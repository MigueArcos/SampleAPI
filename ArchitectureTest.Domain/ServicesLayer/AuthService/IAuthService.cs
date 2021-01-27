using ArchitectureTest.Domain.Models;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.ServiceLayer.AuthService {
    public interface IAuthService {
        Task<JsonWebToken> SignIn(SignInModel signInModel);
        Task<JsonWebToken> SignUp(SignUpModel signUpModel);
    }
}
