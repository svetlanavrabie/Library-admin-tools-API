using System.Net.Http.Headers;
using System.Net.Http.Json;
using LibraryAPI.Client.Models;

namespace LibraryAPI.Client.Services;

public sealed class CourseService(HttpClient client) : ICourseService
{
    private const string JsonMediaType = "application/json";

    public Task<ApiResult<IReadOnlyList<Course>>> GetCoursesAsync(Guid authorId, CancellationToken cancellationToken = default) =>
        ApiCall.SendAsync<IReadOnlyList<Course>>(
            client,
            CreateRequest(HttpMethod.Get, CoursesRoute(authorId)),
            async (response, ct) => await response.Content.ReadFromJsonAsync<List<Course>>(ApiCall.JsonOptions, ct) ?? [],
            cancellationToken);

    public Task<ApiResult<Course>> GetCourseAsync(Guid authorId, Guid courseId, CancellationToken cancellationToken = default) =>
        ApiCall.SendAsync(client, CreateRequest(HttpMethod.Get, CourseRoute(authorId, courseId)), ReadCourseAsync, cancellationToken);

    public Task<ApiResult<Course>> CreateCourseAsync(Guid authorId, CourseForm form, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Post, CoursesRoute(authorId));
        request.Content = JsonContent.Create(ToRequestBody(form));

        return ApiCall.SendAsync(client, request, ReadCourseAsync, cancellationToken);
    }

    public Task<ApiResult> UpdateCourseAsync(Guid authorId, Guid courseId, CourseForm form, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Put, CourseRoute(authorId, courseId));
        request.Content = JsonContent.Create(ToRequestBody(form));

        return ApiCall.SendAsync(client, request, cancellationToken);
    }

    public Task<ApiResult> DeleteCourseAsync(Guid authorId, Guid courseId, CancellationToken cancellationToken = default) =>
        ApiCall.SendAsync(client, CreateRequest(HttpMethod.Delete, CourseRoute(authorId, courseId)), cancellationToken);

    private static string CoursesRoute(Guid authorId) => $"api/authors/{authorId}/courses";

    private static string CourseRoute(Guid authorId, Guid courseId) => $"{CoursesRoute(authorId)}/{courseId}";

    private static HttpRequestMessage CreateRequest(HttpMethod method, string uri)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));
        return request;
    }

    private static object ToRequestBody(CourseForm form) => new
    {
        title = form.Title.Trim(),
        description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim()
    };

    private static async Task<Course> ReadCourseAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        await response.Content.ReadFromJsonAsync<Course>(ApiCall.JsonOptions, cancellationToken)
        ?? throw new System.Text.Json.JsonException("Empty course response.");
}
