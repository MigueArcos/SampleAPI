using ArchitectureTest.Domain.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ArchitectureTest.Domain.Models;

public class SignInModel {
    [Required(ErrorMessage = ErrorCodes.InvalidEmail)]
    [EmailAddress(ErrorMessage = ErrorCodes.InvalidEmail)]
	public required string Email { get; set; }

    [Required(ErrorMessage = ErrorCodes.InvalidPassword)]
    [MinLength(6, ErrorMessage = ErrorCodes.InvalidPassword)]
	public required string Password { get; set; }
}
