using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Client.Models;

public sealed class CourseForm : IValidatableObject
{
    [Required(ErrorMessage = "The title is required.")]
    [MaxLength(100, ErrorMessage = "The title should not have more than 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1500, ErrorMessage = "The description should not have more than 1500 characters.")]
    public string? Description { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Description) && Title == Description)
        {
            yield return new ValidationResult(
                "The description must be different from the title.",
                [nameof(Description)]);
        }
    }
}
