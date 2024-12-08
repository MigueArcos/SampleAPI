using ArchitectureTest.Domain.Models.StatusCodes;
using System.ComponentModel.DataAnnotations;

namespace ArchitectureTest.Domain.Models;

public class SignUpModel : SignInModel {
    [Required(ErrorMessage = ErrorMessages.InvalidUserName)]
	[MinLength(4, ErrorMessage = ErrorMessages.InvalidUserName)]
	public string UserName { get; set; }
}
