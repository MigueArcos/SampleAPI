using System.Security.Claims;
using ArchitectureTest.Domain.Models.Application;

namespace ArchitectureTest.Web.HttpExtensions;

public static class ClaimsIdentityExtensions {
    public static UserTokenIdentity? GetUserIdentity(this IHttpContextAccessor httpContextAccessor) {
        return GetUserIdentity(httpContextAccessor.HttpContext);
    }

    public static UserTokenIdentity? GetUserIdentity(this HttpContext? httpContext)
    {
        if (httpContext?.User != null) {
            var claims = httpContext.User;
            string userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            string email = claims.FindFirst(ClaimTypes.Email)?.Value!;
            string name = claims.FindFirst(ClaimTypes.Name)?.Value!;
            return new UserTokenIdentity { UserId = userId, Email = email, Name = name };
        }
        return null;
    }
}
