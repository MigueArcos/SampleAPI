using ArchitectureTest.Domain.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ArchitectureTest.Domain.Models;

public class SignUpModel : SignInModel {
    [Required(ErrorMessage = ErrorCodes.InvalidUserName)]
	[MinLength(4, ErrorMessage = ErrorCodes.InvalidUserName)]
	public string UserName { get; set; }
}
