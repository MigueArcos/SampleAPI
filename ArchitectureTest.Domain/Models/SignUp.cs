using ArchitectureTest.Domain.StatusCodes;
using System.ComponentModel.DataAnnotations;

namespace ArchitectureTest.Domain.Models {
	public class SignUpModel : SignInModel {
		[MinLength(4, ErrorMessage = CustomMessages.InvalidUserName)]
		public string UserName { get; set; }
	}
}
