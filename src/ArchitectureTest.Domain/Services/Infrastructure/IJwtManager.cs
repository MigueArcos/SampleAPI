using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using System.Security.Claims;

namespace ArchitectureTest.Domain.Services.Infrastructure;

public interface IJwtManager {
    Result<JsonWebToken, AppError> GenerateToken(JwtUser user);
    Result<(JwtUser JwtUser, ClaimsPrincipal Claims), AppError> ReadToken(string token, bool validateLifeTime);
}
