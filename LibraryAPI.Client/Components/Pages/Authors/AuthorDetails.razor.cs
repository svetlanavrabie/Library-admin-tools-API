using Microsoft.AspNetCore.Components;

namespace LibraryAPI.Client.Components.Pages.Authors;

public partial class AuthorDetails
{
    private enum DeleteTarget
    {
        None,
        Author,
        Course
    }

    private Author? _author;
    private IReadOnlyList<Course> _courses = [];
    private bool _isLoading = true;
    private bool _notFound;
    private string? _errorMessage;
    private string? _coursesError;
    private DeleteTarget _confirmTarget;
    private Course? _courseToDelete;
    private bool _isDeleting;

    [Parameter]
    public Guid AuthorId { get; set; }

    [Inject]
    private IAuthorService AuthorService { get; set; } = default!;

    [Inject]
    private ICourseService CourseService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private string ConfirmMessage => _confirmTarget switch
    {
        DeleteTarget.Author => $"Delete {_author?.Name} and all of their courses? This cannot be undone.",
        DeleteTarget.Course => $"Delete the course \"{_courseToDelete?.Title}\"? This cannot be undone.",
        _ => string.Empty
    };

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _author = null;
        _notFound = false;
        _errorMessage = null;

        var result = await AuthorService.GetAuthorAsync(AuthorId);

        if (result.Succeeded)
        {
            _author = result.Value;
            await LoadCoursesAsync();
        }
        else
        {
            _notFound = result.IsNotFound;
            _errorMessage = result.ErrorMessage;
        }

        _isLoading = false;
    }

    private async Task LoadCoursesAsync()
    {
        _coursesError = null;

        var result = await CourseService.GetCoursesAsync(AuthorId);

        if (result.Succeeded)
        {
            _courses = result.Value ?? [];
        }
        else
        {
            _coursesError = result.ErrorMessage;
        }
    }

    private void AskDeleteAuthor() => _confirmTarget = DeleteTarget.Author;

    private void AskDeleteCourse(Course course)
    {
        _courseToDelete = course;
        _confirmTarget = DeleteTarget.Course;
    }

    private void CancelDelete()
    {
        _confirmTarget = DeleteTarget.None;
        _courseToDelete = null;
    }

    private async Task DeleteConfirmedAsync()
    {
        if (_isDeleting)
        {
            return;
        }

        _isDeleting = true;

        if (_confirmTarget == DeleteTarget.Author)
        {
            await DeleteAuthorAsync();
        }
        else if (_courseToDelete is not null)
        {
            await DeleteCourseAsync(_courseToDelete);
        }

        _isDeleting = false;
        CancelDelete();
    }

    private async Task DeleteAuthorAsync()
    {
        var result = await AuthorService.DeleteAuthorAsync(AuthorId);

        if (!result.Succeeded && !result.IsNotFound)
        {
            Notifications.ShowError($"Could not delete the author. {result.ErrorMessage}");
            return;
        }

        Notifications.ShowSuccess($"{_author?.Name} was deleted.");
        Navigation.NavigateTo("authors");
    }

    private async Task DeleteCourseAsync(Course course)
    {
        var result = await CourseService.DeleteCourseAsync(AuthorId, course.Id);

        if (!result.Succeeded && !result.IsNotFound)
        {
            Notifications.ShowError($"Could not delete the course. {result.ErrorMessage}");
            return;
        }

        Notifications.ShowSuccess($"The course \"{course.Title}\" was deleted.");
        await LoadCoursesAsync();
    }
}
