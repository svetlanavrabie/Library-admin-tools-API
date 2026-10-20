using System.Net;
using System.Text.Json;

namespace LibraryAPI.Client.Services;

internal static class ApiCall
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<ApiResult<T>> SendAsync<T>(
        HttpClient client,
        HttpRequestMessage request,
        Func<HttpResponseMessage, CancellationToken, Task<T>> readSuccess,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<T>.Failure(response.StatusCode, await ReadErrorsAsync(response, cancellationToken));
            }

            return ApiResult<T>.Success(response.StatusCode, await readSuccess(response, cancellationToken));
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException
                                   && !cancellationToken.IsCancellationRequested)
        {
            return ApiResult<T>.Failure(null, [DescribeException(ex)]);
        }
    }

    public static async Task<ApiResult> SendAsync(
        HttpClient client,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode
                ? ApiResult.Success(response.StatusCode)
                : ApiResult.Failure(response.StatusCode, await ReadErrorsAsync(response, cancellationToken));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                                   && !cancellationToken.IsCancellationRequested)
        {
            return ApiResult.Failure(null, [DescribeException(ex)]);
        }
    }

    private static string DescribeException(Exception ex) => ex switch
    {
        HttpRequestException => "The library API could not be reached. Check that it is running and that the configured base URL is correct.",
        TaskCanceledException => "The library API did not respond in time.",
        _ => "The library API returned an unexpected response."
    };

    private static async Task<IReadOnlyList<string>> ReadErrorsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(body))
            {
                using var document = JsonDocument.Parse(body);
                CollectValidationErrors(document.RootElement, errors);
            }
        }
        catch (JsonException)
        {
            // Body is not a problem-details document; fall back to the status description below.
        }

        if (errors.Count == 0)
        {
            errors.Add(DescribeStatus(response.StatusCode));
        }

        return errors;
    }

    private static void CollectValidationErrors(JsonElement root, List<string> errors)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in root.EnumerateObject())
        {
            if (!property.NameEquals("errors") || property.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var field in property.Value.EnumerateObject())
            {
                if (field.Value.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                errors.AddRange(field.Value.EnumerateArray()
                    .Select(message => message.GetString())
                    .OfType<string>());
            }
        }
    }

    private static string DescribeStatus(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.NotFound => "The requested record was not found.",
        HttpStatusCode.BadRequest => "The request was rejected as invalid.",
        HttpStatusCode.NotAcceptable => "The API could not provide the requested format.",
        HttpStatusCode.UnsupportedMediaType => "The API does not accept the request format.",
        >= HttpStatusCode.InternalServerError => "The library API encountered an error. Try again later.",
        _ => $"The request failed with status {(int)statusCode}."
    };
}
