using ArchitectureTest.Domain.Models.StatusCodes;
using System.ComponentModel.DataAnnotations;

namespace ArchitectureTest.Domain.Models;

public class SignInModel {
    [Required(ErrorMessage = ErrorMessages.InvalidEmail)]
    [EmailAddress(ErrorMessage = ErrorMessages.InvalidEmail)]
	public string Email { get; set; }

    [Required(ErrorMessage = ErrorMessages.InvalidPassword)]
    [MinLength(6, ErrorMessage = ErrorMessages.InvalidPassword)]
	public string Password { get; set; }
}
