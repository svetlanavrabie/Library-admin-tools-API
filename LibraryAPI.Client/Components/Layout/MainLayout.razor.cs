using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LibraryAPI.Client.Components.Layout;

public partial class MainLayout
{
    private bool _menuOpen;
    private bool _isDark;

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _isDark = await Js.InvokeAsync<string>("libraryTheme.get") == "dark";
        StateHasChanged();
    }

    private async Task ToggleThemeAsync()
    {
        _isDark = await Js.InvokeAsync<string>("libraryTheme.toggle") == "dark";
    }

    private void ToggleMenu() => _menuOpen = !_menuOpen;

    private void CloseMenu() => _menuOpen = false;
}
