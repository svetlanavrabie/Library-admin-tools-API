using Microsoft.AspNetCore.Components;

namespace LibraryAPI.Client.Components.Shared;

public enum StateKind
{
    Loading,
    Empty,
    Error
}

public partial class StatePanel
{
    [Parameter, EditorRequired]
    public StateKind Kind { get; set; }

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
