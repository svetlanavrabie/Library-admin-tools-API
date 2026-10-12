using Microsoft.AspNetCore.Components;

namespace LibraryAPI.Client.Components.Shared;

public partial class ConfirmDialog
{
    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public bool IsBusy { get; set; }

    [Parameter]
    public string Title { get; set; } = "Please confirm";

    [Parameter]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public string ConfirmText { get; set; } = "Delete";

    [Parameter]
    public EventCallback OnConfirm { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    private Task Confirm() => IsBusy ? Task.CompletedTask : OnConfirm.InvokeAsync();

    private Task Cancel() => IsBusy ? Task.CompletedTask : OnCancel.InvokeAsync();
}
