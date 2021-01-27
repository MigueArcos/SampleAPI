using ArchitectureTest.Domain.StatusCodes;
using System.ComponentModel.DataAnnotations;

namespace ArchitectureTest.Domain.Models {
	public class SignUpModel : SignInModel {
        [Required(ErrorMessage = CustomMessages.InvalidUserName)]
		[MinLength(4, ErrorMessage = CustomMessages.InvalidUserName)]
		public string UserName { get; set; }
	}
}
