namespace LibraryAPI.Client.Models;

public sealed record AuthorQuery(
    string? Search = null,
    string? MainCategory = null,
    string OrderBy = "Name",
    int PageNumber = 1,
    int PageSize = 10);
