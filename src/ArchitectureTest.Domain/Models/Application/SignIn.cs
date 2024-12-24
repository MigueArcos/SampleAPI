using System.ComponentModel.DataAnnotations;
using ArchitectureTest.Domain.Errors;

namespace ArchitectureTest.Domain.Models.Application;

public class SignInModel {
    [Required(ErrorMessage = ErrorCodes.InvalidEmail)]
    [EmailAddress(ErrorMessage = ErrorCodes.InvalidEmail)]
    public required string Email { get; set; }

    [Required(ErrorMessage = ErrorCodes.InvalidPassword)]
    [MinLength(6, ErrorMessage = ErrorCodes.InvalidPassword)]
    public required string Password { get; set; }
}
