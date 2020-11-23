using ArchitectureTest.Domain.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Contracts {
	public interface IJwtManager {
		Task<JsonWebToken> GenerateToken(JwtUser user);
		ClaimsPrincipal ReadToken(string token, bool validateLifeTime);
		Task<JwtWithClaims> ExchangeRefreshToken(string accessToken, string refreshToken);
	}
}
