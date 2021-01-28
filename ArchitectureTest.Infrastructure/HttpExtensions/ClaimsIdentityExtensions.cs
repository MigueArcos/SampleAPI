using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ArchitectureTest.Infrastructure.HttpExtensions {
    public static class ClaimsIdentityExtensions {
        public static (long UserId, string Email, string Name) GetUserIdentity(this IHttpContextAccessor httpContextAccessor) {
            if (httpContextAccessor.HttpContext.User != null) {
                var claims = httpContextAccessor.HttpContext.User;
                long userId = long.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                string email = claims.FindFirst(ClaimTypes.Email)?.Value;
                string name = claims.FindFirst(ClaimTypes.Name)?.Value;
                return (userId, email, name);
            }
            return (0, string.Empty, string.Empty);
        }
    }
}
