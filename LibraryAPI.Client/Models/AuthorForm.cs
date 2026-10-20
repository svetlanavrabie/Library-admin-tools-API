using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Client.Models;

public sealed class AuthorForm
{
    [Required(ErrorMessage = "The first name is required.")]
    [MaxLength(50, ErrorMessage = "The first name should not have more than 50 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "The last name is required.")]
    [MaxLength(50, ErrorMessage = "The last name should not have more than 50 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "The date of birth is required.")]
    public DateTime? DateOfBirth { get; set; }

    public DateTime? DateOfDeath { get; set; }

    [Required(ErrorMessage = "The main category is required.")]
    [MaxLength(50, ErrorMessage = "The main category should not have more than 50 characters.")]
    public string MainCategory { get; set; } = string.Empty;
}
