using Microsoft.AspNetCore.Components;

namespace LibraryAPI.Client.Components.Shared;

public partial class Pagination
{
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    [Parameter]
    public int TotalPages { get; set; } = 1;

    [Parameter]
    public EventCallback<int> OnPageChanged { get; set; }

    private bool HasPrevious => CurrentPage > 1;

    private bool HasNext => CurrentPage < TotalPages;

    private Task GoTo(int page) => OnPageChanged.InvokeAsync(page);
}
