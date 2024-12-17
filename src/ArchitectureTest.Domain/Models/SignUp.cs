using System.ComponentModel.DataAnnotations;
using ArchitectureTest.Domain.Errors;

namespace ArchitectureTest.Domain.Models;

public class SignUpModel : SignInModel {
    [Required(ErrorMessage = ErrorCodes.InvalidUserName)]
    [MinLength(4, ErrorMessage = ErrorCodes.InvalidUserName)]
    public required string UserName { get; set; }
}
