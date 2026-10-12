namespace LibraryAPI.Client.Components.Layout;

public partial class MainLayout
{
    private bool _menuOpen;

    private void ToggleMenu() => _menuOpen = !_menuOpen;

    private void CloseMenu() => _menuOpen = false;
}
