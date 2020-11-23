using ArchitectureTest.Domain.Contracts;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Domain.UnitOfWork;
using ArchitectureTest.Infrastructure.Extensions;
using ArchitectureTest.Infrastructure.Helpers;
using ArchitectureTest.Infrastructure.HttpExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	public class LoginController : ControllerBase {
		private readonly UsersDomain usersDomain;

		public LoginController(IUnitOfWork unitOfWork, IJwtManager jwtManager, IPasswordHasher passwordHasher) {
			usersDomain = new UsersDomain(unitOfWork, jwtManager, passwordHasher);
		}

		// POST api/values
		[HttpPost("sign-in")]
		public async Task<IActionResult> SignIn([FromBody] SignInModel signInModel) {
			if (ModelState.IsValid){
				try{
					var token = await usersDomain.SignIn(signInModel);
					bool saveAuthInCookie = HttpContext.Request.Headers[AppConstants.SaveAuthInCookieHeader] == "true";
					if (saveAuthInCookie) {
						HttpContext.SetCookie(AppConstants.SessionCookie, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string> {
							[AppConstants.Token] = token.Token,
							[AppConstants.RefreshToken] = token.RefreshToken
						}));
					}
					return Ok(token);
				}
				catch (ErrorStatusCode error) {
					return new ObjectResult(error.StatusCode) {
						StatusCode = error.HttpStatusCode
					};
				}
			}
			else{
				var errors = ModelState.GetErrors();
				return BadRequest(new {
					StatusCode = 400,
					Message = errors[0]
				});
			}
		}
		[HttpPost("sign-up")]
		public async Task<IActionResult> SignUp([FromBody] SignUpModel signUpModel) {
			if (ModelState.IsValid) {
				try {
					var token = await usersDomain.SignUp(signUpModel);
					bool saveAuthInCookie = HttpContext.Request.Headers[AppConstants.SaveAuthInCookieHeader] == "true";
					if (saveAuthInCookie) {
						HttpContext.SetCookie(AppConstants.SessionCookie, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string> {
							[AppConstants.Token] = token.Token,
							[AppConstants.RefreshToken] = token.RefreshToken
						}));
					}
					return Ok(token);
				}
				catch (ErrorStatusCode error) {
					return new ObjectResult(error.StatusCode) {
						StatusCode = error.HttpStatusCode
					};
				}
			}
			else {
				var errors = ModelState.GetErrors();
				return BadRequest(new {
					StatusCode = 400,
					Message = errors[0]
				});
			}
		}
	}
	
}
