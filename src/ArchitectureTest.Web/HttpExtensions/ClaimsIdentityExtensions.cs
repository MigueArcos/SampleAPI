using System.Security.Claims;

namespace ArchitectureTest.Web.HttpExtensions;

public static class ClaimsIdentityExtensions {
    public static (long UserId, string Email, string Name) GetUserIdentity(this IHttpContextAccessor httpContextAccessor) {
        if (httpContextAccessor.HttpContext?.User != null) {
            var claims = httpContextAccessor.HttpContext.User;
            long userId = long.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            string email = claims.FindFirst(ClaimTypes.Email)?.Value ?? "user@default.io";
            string name = claims.FindFirst(ClaimTypes.Name)?.Value ?? "Default";
            return (userId, email, name);
        }
        return (0, string.Empty, string.Empty);
    }
}
