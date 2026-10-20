using System.Net;

namespace LibraryAPI.Client.Services;

public class ApiResult
{
    protected ApiResult(bool succeeded, HttpStatusCode? statusCode, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        StatusCode = statusCode;
        Errors = errors;
    }

    public bool Succeeded { get; }
    public HttpStatusCode? StatusCode { get; }
    public IReadOnlyList<string> Errors { get; }
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;
    public string ErrorMessage => string.Join(" ", Errors);

    public static ApiResult Success(HttpStatusCode statusCode) => new(true, statusCode, []);

    public static ApiResult Failure(HttpStatusCode? statusCode, IReadOnlyList<string> errors) =>
        new(false, statusCode, errors);
}

public sealed class ApiResult<T> : ApiResult
{
    private ApiResult(bool succeeded, HttpStatusCode? statusCode, IReadOnlyList<string> errors, T? value)
        : base(succeeded, statusCode, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static ApiResult<T> Success(HttpStatusCode statusCode, T value) => new(true, statusCode, [], value);

    public new static ApiResult<T> Failure(HttpStatusCode? statusCode, IReadOnlyList<string> errors) =>
        new(false, statusCode, errors, default);
}
