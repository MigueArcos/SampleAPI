using ArchitectureTest.Domain.Models;
using System.Security.Claims;

namespace ArchitectureTest.Domain.Contracts {
    public interface IJwtManager {
        JsonWebToken GenerateToken(JwtUser user);
        (ClaimsPrincipal claims, JwtUser jwtUser) ReadToken(string token, bool validateLifeTime);
        int TokenTTLSeconds { get; }
        int RefreshTokenTTLSeconds { get; }
    }
}
