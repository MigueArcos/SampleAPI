using ArchitectureTest.Domain.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Contracts {
    public interface IJwtManager {
        JsonWebToken GenerateToken(JwtUser user);
        ClaimsPrincipal ReadToken(string token, bool validateLifeTime);
        JwtWithClaims ExchangeRefreshToken(string accessToken, string refreshToken);
        int TokenTTLSeconds { get; }
        int RefreshTokenTTLSeconds { get; }
    }
}
