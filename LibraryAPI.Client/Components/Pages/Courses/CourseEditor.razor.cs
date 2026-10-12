using Microsoft.AspNetCore.Components;

namespace LibraryAPI.Client.Components.Pages.Courses;

public partial class CourseEditor
{
    private CourseForm _form = new();
    private IReadOnlyList<string> _serverErrors = [];
    private bool _isLoading;
    private bool _isSubmitting;
    private string? _loadError;

    [Parameter]
    public Guid AuthorId { get; set; }

    [Parameter]
    public Guid? CourseId { get; set; }

    [Inject]
    private ICourseService CourseService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private bool IsEdit => CourseId.HasValue;

    private string PageHeading => IsEdit ? "Edit course" : "New course";

    protected override async Task OnParametersSetAsync()
    {
        _loadError = null;
        _form = new CourseForm();

        if (CourseId is not { } courseId)
        {
            return;
        }

        _isLoading = true;

        var result = await CourseService.GetCourseAsync(AuthorId, courseId);

        if (result.Succeeded)
        {
            _form = new CourseForm { Title = result.Value!.Title, Description = result.Value.Description };
        }
        else
        {
            _loadError = result.ErrorMessage;
        }

        _isLoading = false;
    }

    private async Task SubmitAsync()
    {
        if (_isSubmitting)
        {
            return;
        }

        _isSubmitting = true;
        _serverErrors = [];

        var result = CourseId is { } courseId
            ? await CourseService.UpdateCourseAsync(AuthorId, courseId, _form)
            : await CreateAsync();

        _isSubmitting = false;

        if (!result.Succeeded)
        {
            _serverErrors = result.Errors;
            Notifications.ShowError("The course could not be saved.");
            return;
        }

        Notifications.ShowSuccess(IsEdit ? "The course was updated." : "The course was created.");
        Navigation.NavigateTo($"authors/{AuthorId}");
    }

    private async Task<ApiResult> CreateAsync() => await CourseService.CreateCourseAsync(AuthorId, _form);
}
