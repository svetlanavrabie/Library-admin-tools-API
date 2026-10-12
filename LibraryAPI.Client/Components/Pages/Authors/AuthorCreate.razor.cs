using Microsoft.AspNetCore.Components;

namespace LibraryAPI.Client.Components.Pages.Authors;

public partial class AuthorCreate
{
    private readonly AuthorForm _form = new();
    private IReadOnlyList<string> _serverErrors = [];
    private bool _isSubmitting;

    [Inject]
    private IAuthorService AuthorService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    private async Task SubmitAsync()
    {
        if (_isSubmitting)
        {
            return;
        }

        _isSubmitting = true;
        _serverErrors = [];

        var result = await AuthorService.CreateAuthorAsync(_form);

        _isSubmitting = false;

        if (!result.Succeeded)
        {
            _serverErrors = result.Errors;
            Notifications.ShowError("The author could not be created.");
            return;
        }

        Notifications.ShowSuccess($"{result.Value!.Name} was created.");
        Navigation.NavigateTo($"authors/{result.Value.Id}");
    }
}
