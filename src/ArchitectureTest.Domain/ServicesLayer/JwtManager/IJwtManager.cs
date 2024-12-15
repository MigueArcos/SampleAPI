using ArchitectureTest.Domain.Models;
using System.Security.Claims;

namespace ArchitectureTest.Domain.ServiceLayer.JwtManager;

public interface IJwtManager {
    Result<JsonWebToken, AppError> GenerateToken(JwtUser user);
    Result<(ClaimsPrincipal Claims, JwtUser JwtUser), AppError> ReadToken(string token, bool validateLifeTime);
    int TokenTTLSeconds { get; }
    int RefreshTokenTTLSeconds { get; }
}
