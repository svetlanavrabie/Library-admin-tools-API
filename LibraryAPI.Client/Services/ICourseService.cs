using LibraryAPI.Client.Models;

namespace LibraryAPI.Client.Services;

public interface ICourseService
{
    Task<ApiResult<IReadOnlyList<Course>>> GetCoursesAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<ApiResult<Course>> GetCourseAsync(Guid authorId, Guid courseId, CancellationToken cancellationToken = default);
    Task<ApiResult<Course>> CreateCourseAsync(Guid authorId, CourseForm form, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateCourseAsync(Guid authorId, Guid courseId, CourseForm form, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteCourseAsync(Guid authorId, Guid courseId, CancellationToken cancellationToken = default);
}
