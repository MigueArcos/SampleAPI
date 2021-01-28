using ArchitectureTest.Domain.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace ArchitectureTest.Web.Services.UserIdentity {
	public class ClaimsUserAccesor : IClaimsUserAccesor<JwtUser> {
		private readonly IHttpContextAccessor httpContextAccessor;

		public ClaimsUserAccesor(IHttpContextAccessor httpContextAccessor) {
			this.httpContextAccessor = httpContextAccessor;
		}

		public JwtUser GetUser() {
			if (httpContextAccessor.HttpContext.User != null){
				var claims = httpContextAccessor.HttpContext.User;
				return new JwtUser {
					Email = claims.FindFirst(ClaimTypes.Email)?.Value,
					Id = long.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value),
					Name = claims.FindFirst(ClaimTypes.Name)?.Value
				};
			}
			return null;
		}
	}
}
