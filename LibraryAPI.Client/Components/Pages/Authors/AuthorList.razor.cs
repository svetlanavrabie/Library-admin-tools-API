using Microsoft.AspNetCore.Components;

namespace LibraryAPI.Client.Components.Pages.Authors;

public partial class AuthorList
{
    private const string DescendingSuffix = " desc";

    private PagedResult<Author>? _page;
    private AuthorQuery _query = new();
    private string _searchText = string.Empty;
    private string _categoryText = string.Empty;
    private string? _errorMessage;
    private bool _isLoading;
    private Author? _authorToDelete;
    private bool _isDeleting;

    [Inject]
    private IAuthorService AuthorService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    private bool HasFilters => !string.IsNullOrWhiteSpace(_query.Search) || !string.IsNullOrWhiteSpace(_query.MainCategory);

    protected override Task OnInitializedAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        var result = await AuthorService.GetAuthorsAsync(_query);

        if (result.Succeeded)
        {
            _page = result.Value;
        }
        else
        {
            _errorMessage = result.ErrorMessage;
        }

        _isLoading = false;
    }

    private Task ApplyFiltersAsync()
    {
        _query = _query with
        {
            Search = _searchText.Trim(),
            MainCategory = _categoryText.Trim(),
            PageNumber = 1
        };

        return LoadAsync();
    }

    private Task ClearFiltersAsync()
    {
        _searchText = string.Empty;
        _categoryText = string.Empty;

        return ApplyFiltersAsync();
    }

    private Task GoToPageAsync(int pageNumber)
    {
        _query = _query with { PageNumber = pageNumber };
        return LoadAsync();
    }

    private Task SortByAsync(string column)
    {
        var orderBy = _query.OrderBy == column ? column + DescendingSuffix : column;
        _query = _query with { OrderBy = orderBy, PageNumber = 1 };

        return LoadAsync();
    }

    private string SortIndicator(string column)
    {
        if (_query.OrderBy == column)
        {
            return "\u25B2";
        }

        return _query.OrderBy == column + DescendingSuffix ? "\u25BC" : string.Empty;
    }

    private void AskDelete(Author author) => _authorToDelete = author;

    private void CancelDelete() => _authorToDelete = null;

    private async Task DeleteConfirmedAsync()
    {
        if (_authorToDelete is null || _isDeleting)
        {
            return;
        }

        _isDeleting = true;
        var author = _authorToDelete;

        var result = await AuthorService.DeleteAuthorAsync(author.Id);

        _isDeleting = false;
        _authorToDelete = null;

        if (!result.Succeeded && !result.IsNotFound)
        {
            Notifications.ShowError($"Could not delete {author.Name}. {result.ErrorMessage}");
            return;
        }

        Notifications.ShowSuccess($"{author.Name} was deleted.");

        if (_page is { Items.Count: 1, CurrentPage: > 1 })
        {
            _query = _query with { PageNumber = _query.PageNumber - 1 };
        }

        await LoadAsync();
    }
}
