using ArchitectureTest.Infrastructure.Jwt.Models;
using System.Security.Claims;

namespace ArchitectureTest.Infrastructure.Jwt {
	public interface IJwtManager {
		JsonWebToken GenerateToken(JwtUser user);
		string GenerateRefreshToken();
		ClaimsPrincipal ReadToken(string token, bool validateLifeTime);
		JwtWithClaims ExchangeRefreshToken(string accessToken, string refreshToken);
	}
}
