using LibraryAPI.Client.Models;

namespace LibraryAPI.Client.Services;

public interface IAuthorService
{
    Task<ApiResult<PagedResult<Author>>> GetAuthorsAsync(AuthorQuery query, CancellationToken cancellationToken = default);
    Task<ApiResult<Author>> GetAuthorAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<ApiResult<Author>> CreateAuthorAsync(AuthorForm form, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteAuthorAsync(Guid authorId, CancellationToken cancellationToken = default);
}
