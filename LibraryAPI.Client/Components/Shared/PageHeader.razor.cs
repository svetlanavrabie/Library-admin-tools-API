using Microsoft.AspNetCore.Components;

namespace LibraryAPI.Client.Components.Shared;

public partial class PageHeader
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public string? BackHref { get; set; }

    [Parameter]
    public string BackText { get; set; } = "Back";

    [Parameter]
    public RenderFragment? Actions { get; set; }
}
