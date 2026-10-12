using Microsoft.AspNetCore.Components;

namespace LibraryAPI.Client.Components.Layout;

public partial class NavMenu
{
    [Parameter]
    public EventCallback OnNavigate { get; set; }

    private Task NotifyNavigate() => OnNavigate.InvokeAsync();
}
