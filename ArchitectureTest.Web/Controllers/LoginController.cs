using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Infrastructure.Extensions;
using ArchitectureTest.Infrastructure.Helpers;
using ArchitectureTest.Infrastructure.Jwt;
using ArchitectureTest.Infrastructure.Jwt.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class LoginController : ControllerBase {
		private readonly IJwtManager jwtManager;
		private readonly IHttpContextAccessor httpContextAccessor;

		public LoginController(IJwtManager jwtManager, IHttpContextAccessor httpContextAccessor) {
			this.jwtManager = jwtManager;
			this.httpContextAccessor = httpContextAccessor;
		}

		// POST api/values
		[HttpPost("sign-in")]
		public ObjectResult SignIn([FromBody] SignInModel signInModel) {
			if (signInModel.Email == "migue300995@gmail.com" && signInModel.Password == "zeusensacion"){
				var token = jwtManager.GenerateToken(new JwtUser { 
					Name = "Miguel Angel López Arcos",
					Email = "migue300995@gmail.com",
					Id = 1
				});
				httpContextAccessor.HttpContext.SetCookie(AppConstants.SessionCookie, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string> {
					[AppConstants.Token] = token.Token,
					[AppConstants.RefreshToken] = token.RefreshToken
				}));
				return Ok(token);
			}else{
				return BadRequest(ErrorStatusCode.AuthorizationFailed.StatusCode);
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
