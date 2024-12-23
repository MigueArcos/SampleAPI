using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Application;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Services.Application.AuthService;

public interface IAuthService {
    Task<Result<JsonWebToken, AppError>> SignIn(SignInModel signInModel);
    Task<Result<JsonWebToken, AppError>> SignUp(SignUpModel signUpModel);
    Task<Result<(JsonWebToken Token, ClaimsPrincipal Claims), AppError>> ExchangeOldTokensForNewToken(
        string token, string refreshToken
    );
}
