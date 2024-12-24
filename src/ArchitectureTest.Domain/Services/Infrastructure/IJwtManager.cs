using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Application;
using System.Security.Claims;

namespace ArchitectureTest.Domain.Services.Infrastructure;

public interface IJwtManager {
    Result<JsonWebToken, AppError> GenerateToken(UserTokenIdentity identity);
    Result<(UserTokenIdentity Identity, ClaimsPrincipal Claims), AppError> ReadToken(string token, bool validateLifeTime);
}
