using Microsoft.AspNetCore.Components;

namespace LibraryAPI.Client.Components.Shared;

public partial class ToastHost : IDisposable
{
    private static readonly TimeSpan AutoDismissDelay = TimeSpan.FromSeconds(5);

    private int _knownCount;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    protected override void OnInitialized() => Notifications.Changed += OnNotificationsChanged;

    private void OnNotificationsChanged()
    {
        var added = Notifications.Notifications.Skip(_knownCount).ToList();
        _knownCount = Notifications.Notifications.Count;

        foreach (var notification in added)
        {
            _ = DismissLaterAsync(notification.Id);
        }

        _ = InvokeAsync(StateHasChanged);
    }

    private async Task DismissLaterAsync(Guid id)
    {
        await Task.Delay(AutoDismissDelay);
        Notifications.Dismiss(id);
    }

    public void Dispose() => Notifications.Changed -= OnNotificationsChanged;
}
