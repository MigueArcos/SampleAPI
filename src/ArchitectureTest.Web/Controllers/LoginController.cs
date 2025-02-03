using System.Text.Json;
using ArchitectureTest.Web.Configuration;
using ArchitectureTest.Infrastructure.HttpExtensions;
using ArchitectureTest.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services.Application.AuthService;
using ArchitectureTest.Domain.Models.Application;

namespace ArchitectureTest.Web.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
public class LoginController : BaseController {
    private readonly IAuthService _authService;

    public LoginController(IAuthService authService, ILogger<LoginController> logger) : base(logger) {
        _authService = authService;
    }

    // POST api/values
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn(
        [FromBody] SignInModel signInModel,
        [FromHeader(Name = AppConstants.SaveAuthInCookieHeader)] bool saveAuthInCookie
    ) {
        if (ModelState.IsValid){
            var tokenResult = await _authService.SignIn(signInModel).ConfigureAwait(false);
            if (tokenResult.Error is not null)
                return HandleError(tokenResult.Error);

            var token = tokenResult.Value!;
            
            if (saveAuthInCookie)
                SaveAuthInCookie(token.Token, token.RefreshToken);

            return Ok(token);
        }
        else{
            var errors = ModelState.GetErrors();
            return BadRequest(new BadRequestHttpErrorInfo {
                ErrorCode = ErrorCodes.ValidationsFailed,
                Errors = errors.Select(e => new HttpErrorInfo { ErrorCode = e }).ToList()
            });
        }
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp(
        [FromBody] SignUpModel signUpModel,
        [FromHeader(Name = AppConstants.SaveAuthInCookieHeader)] bool saveAuthInCookie
    ) {
        if (ModelState.IsValid) {
            var tokenResult = await _authService.SignUp(signUpModel).ConfigureAwait(false);
            if (tokenResult.Error is not null)
                return HandleError(tokenResult.Error);

            var token = tokenResult.Value!;

            if (saveAuthInCookie)
                SaveAuthInCookie(token.Token, token.RefreshToken);

            return Ok(token);
        }
        else {
            var errors = ModelState.GetErrors();
            return BadRequest(new BadRequestHttpErrorInfo {
                ErrorCode = ErrorCodes.ValidationsFailed,
                Errors = errors.Select(e => new HttpErrorInfo { ErrorCode = e }).ToList()
            });
        }
    }

    private void SaveAuthInCookie(string token, string refreshToken)
    {
        HttpContext.SetResponseCookie(
            AppConstants.SessionCookie,
            JsonSerializer.Serialize(new Dictionary<string, string> {
                [AppConstants.Token] = token,
                [AppConstants.RefreshToken] = refreshToken
            })
        );
    }
}

