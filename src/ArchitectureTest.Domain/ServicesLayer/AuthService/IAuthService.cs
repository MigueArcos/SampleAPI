using ArchitectureTest.Domain.Models;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.ServiceLayer.AuthService;

public interface IAuthService {
    Task<Result<JsonWebToken, AppError>> SignIn(SignInModel signInModel);
    Task<Result<JsonWebToken, AppError>> SignUp(SignUpModel signUpModel);
}
