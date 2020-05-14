using ArchitectureTest.Infrastructure.Extensions;
using ArchitectureTest.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class LoginController : ControllerBase {
		private readonly JwtTokenManager jwtTokenManager;
		private readonly IHttpContextAccessor httpContextAccessor;

		public LoginController(JwtTokenManager jwtTokenManager, IHttpContextAccessor httpContextAccessor) {
			this.jwtTokenManager = jwtTokenManager;
			this.httpContextAccessor = httpContextAccessor;
		}

		// POST api/values
		[HttpPost("sign-in")]
		public ObjectResult SignIn([FromBody] SignInModel signInModel) {
			if (signInModel.Email == "migue300995@gmail.com" && signInModel.Password == "zeusensacion"){
				var token = jwtTokenManager.GenerateToken(1);
				httpContextAccessor.HttpContext.SetCookie(AppConstants.JwtCookieName, token);
				return Ok(new { Status = "Login succesful" });
			}else{
				return Ok(new { Status = "Login failed" });
			}
		}
		[HttpPost("sign-up")]
		public void SignUp([FromBody] string value) {
		}
	}
	public class SignInModel {
		public string Email { get; set; }
		public string Password { get; set; }
	}
}
