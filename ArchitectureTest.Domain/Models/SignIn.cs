using ArchitectureTest.Domain.StatusCodes;
using System.ComponentModel.DataAnnotations;

namespace ArchitectureTest.Domain.Models {
	public class SignInModel {
        [Required(ErrorMessage = CustomMessages.InvalidEmail)]
        [EmailAddress(ErrorMessage = CustomMessages.InvalidEmail)]
		public string Email { get; set; }
        [Required(ErrorMessage = CustomMessages.InvalidEmail)]
        [MinLength(6, ErrorMessage = CustomMessages.InvalidPassword)]
		public string Password { get; set; }
	}
}
