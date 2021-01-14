using ArchitectureTest.Domain.Models;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Domain {
    public interface IUsersDomain {
        Task<JsonWebToken> SignIn(SignInModel signInModel);
        Task<JsonWebToken> SignUp(SignUpModel signUpModel);
    }
}
