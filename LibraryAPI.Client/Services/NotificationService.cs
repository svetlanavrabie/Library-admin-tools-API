namespace LibraryAPI.Client.Services;

public enum NotificationKind
{
    Success,
    Error
}

public sealed record Notification(Guid Id, NotificationKind Kind, string Message);

public interface INotificationService
{
    event Action? Changed;
    IReadOnlyList<Notification> Notifications { get; }
    void ShowSuccess(string message);
    void ShowError(string message);
    void Dismiss(Guid id);
}

public sealed class NotificationService : INotificationService
{
    private readonly List<Notification> _notifications = [];

    public event Action? Changed;

    public IReadOnlyList<Notification> Notifications => _notifications;

    public void ShowSuccess(string message) => Add(NotificationKind.Success, message);

    public void ShowError(string message) => Add(NotificationKind.Error, message);

    public void Dismiss(Guid id)
    {
        if (_notifications.RemoveAll(n => n.Id == id) > 0)
        {
            Changed?.Invoke();
        }
    }

    private void Add(NotificationKind kind, string message)
    {
        _notifications.Add(new Notification(Guid.NewGuid(), kind, message));
        Changed?.Invoke();
    }
}
