using ArchitectureTest.Domain.StatusCodes;
using System.ComponentModel.DataAnnotations;

namespace ArchitectureTest.Domain.Models {
	public class SignInModel {
		[EmailAddress(ErrorMessage = CustomMessages.InvalidEmail)]
		public string Email { get; set; }
		[MinLength(6, ErrorMessage = CustomMessages.InvalidPassword)]
		public string Password { get; set; }
	}
}
