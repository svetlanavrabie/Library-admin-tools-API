using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LibraryAPI.Client.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace LibraryAPI.Client.Services;

public sealed class AuthorService(HttpClient client) : IAuthorService
{
    private const string AuthorsRoute = "api/authors";
    private const string JsonMediaType = "application/json";
    private const string DateOfDeathMediaType = "application/vdn.marvin.authorforcreationwithdateofdeath+json";
    private const string PaginationHeader = "X-Pagination";

    public Task<ApiResult<PagedResult<Author>>> GetAuthorsAsync(AuthorQuery query, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["search"] = query.Search,
            ["mainCategory"] = query.MainCategory,
            ["orderBy"] = query.OrderBy,
            ["pageNumber"] = query.PageNumber.ToString(),
            ["pageSize"] = query.PageSize.ToString()
        };

        var uri = QueryHelpers.AddQueryString(AuthorsRoute,
            parameters.Where(p => !string.IsNullOrWhiteSpace(p.Value)));

        return ApiCall.SendAsync(client, CreateGet(uri), ReadPageAsync, cancellationToken);
    }

    public Task<ApiResult<Author>> GetAuthorAsync(Guid authorId, CancellationToken cancellationToken = default) =>
        ApiCall.SendAsync(client, CreateGet($"{AuthorsRoute}/{authorId}"), ReadAuthorAsync, cancellationToken);

    public Task<ApiResult<Author>> CreateAuthorAsync(AuthorForm form, CancellationToken cancellationToken = default)
    {
        var hasDateOfDeath = form.DateOfDeath.HasValue;

        var body = new Dictionary<string, object?>
        {
            ["firstName"] = form.FirstName.Trim(),
            ["lastName"] = form.LastName.Trim(),
            ["dateOfBirth"] = ToUtcOffset(form.DateOfBirth!.Value),
            ["mainCategory"] = form.MainCategory.Trim()
        };

        if (hasDateOfDeath)
        {
            body["dateOfDeath"] = ToUtcOffset(form.DateOfDeath!.Value);
        }

        var request = new HttpRequestMessage(HttpMethod.Post, AuthorsRoute)
        {
            Content = CreateJsonContent(body, hasDateOfDeath ? DateOfDeathMediaType : JsonMediaType)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));

        return ApiCall.SendAsync(client, request, ReadAuthorAsync, cancellationToken);
    }

    public Task<ApiResult> DeleteAuthorAsync(Guid authorId, CancellationToken cancellationToken = default) =>
        ApiCall.SendAsync(client, new HttpRequestMessage(HttpMethod.Delete, $"{AuthorsRoute}/{authorId}"), cancellationToken);

    private static HttpRequestMessage CreateGet(string uri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));
        return request;
    }

    private static DateTimeOffset ToUtcOffset(DateTime date) =>
        new(DateTime.SpecifyKind(date.Date, DateTimeKind.Utc));

    // The API matches the Content-Type header exactly, so no charset parameter may be appended.
    private static ByteArrayContent CreateJsonContent(object body, string mediaType)
    {
        var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(body, ApiCall.JsonOptions));
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        return content;
    }

    private static async Task<Author> ReadAuthorAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        await response.Content.ReadFromJsonAsync<Author>(ApiCall.JsonOptions, cancellationToken)
        ?? throw new JsonException("Empty author response.");

    private static async Task<PagedResult<Author>> ReadPageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var envelope = await response.Content.ReadFromJsonAsync<AuthorEnvelope>(ApiCall.JsonOptions, cancellationToken);
        var items = envelope?.Value ?? [];
        var pagination = ReadPagination(response);

        return new PagedResult<Author>
        {
            Items = items,
            TotalCount = pagination?.TotalCount ?? items.Count,
            PageSize = pagination?.PageSize ?? items.Count,
            CurrentPage = pagination?.CurrentPage ?? 1,
            TotalPages = pagination?.TotalPages ?? 1
        };
    }

    private static PaginationMetadata? ReadPagination(HttpResponseMessage response) =>
        response.Headers.TryGetValues(PaginationHeader, out var values)
            ? JsonSerializer.Deserialize<PaginationMetadata>(values.First(), ApiCall.JsonOptions)
            : null;

    private sealed record AuthorEnvelope(List<Author>? Value);

    private sealed record PaginationMetadata(int TotalCount, int PageSize, int CurrentPage, int TotalPages);
}
