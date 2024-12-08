using System.Text.Json;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.StatusCodes;
using ArchitectureTest.Domain.ServiceLayer.AuthService;
using ArchitectureTest.Web.Configuration;
using ArchitectureTest.Infrastructure.HttpExtensions;
using ArchitectureTest.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

[Route("api/[controller]")]
public class LoginController : BaseController {
	private readonly IAuthService usersDomain;

	public LoginController(IAuthService usersDomain) {
		this.usersDomain = usersDomain;
	}

	// POST api/values
	[HttpPost("sign-in")]
	public async Task<IActionResult> SignIn(
		[FromBody] SignInModel signInModel,
		[FromHeader(Name = AppConstants.SaveAuthInCookieHeader)] bool saveAuthInCookie
	) {
		if (ModelState.IsValid){
			try{
				var token = await usersDomain.SignIn(signInModel);
				// bool saveAuthInCookie = HttpContext.Request.Headers[AppConstants.SaveAuthInCookieHeader] == "true";
				if (saveAuthInCookie) {
					HttpContext.SetCookie(
						AppConstants.SessionCookie, 
						JsonSerializer.Serialize(new Dictionary<string, string> {
							[AppConstants.Token] = token.Token,
							[AppConstants.RefreshToken] = token.RefreshToken
						})
					);
				}
				return Ok(token);
			}
			catch (Exception error) {
				return DefaultCatch(error);
			}
		}
		else{
			var errors = ModelState.GetErrors();
			return BadRequest(new ErrorDetail {
				ErrorCode = ErrorCodes.ValidationsFailed,
				Message = errors[0]
			});
		}
	}

	[HttpPost("sign-up")]
	public async Task<IActionResult> SignUp(
		[FromBody] SignUpModel signUpModel,
		[FromHeader(Name = AppConstants.SaveAuthInCookieHeader)] bool saveAuthInCookie
	) {
		if (ModelState.IsValid) {
			try {
				var token = await usersDomain.SignUp(signUpModel);
				// bool saveAuthInCookie = HttpContext.Request.Headers[AppConstants.SaveAuthInCookieHeader] == "true";
				if (saveAuthInCookie) {
					HttpContext.SetCookie(
						AppConstants.SessionCookie,
						JsonSerializer.Serialize(new Dictionary<string, string> {
							[AppConstants.Token] = token.Token,
							[AppConstants.RefreshToken] = token.RefreshToken
						})
					);
				}
				return Ok(token);
			}
			catch (Exception error) {
				return DefaultCatch(error);
			}
		}
		else {
			var errors = ModelState.GetErrors();
			return BadRequest(new ErrorDetail {
				ErrorCode = ErrorCodes.ValidationsFailed,
				Message = errors[0]
			});
		}
	}
}

