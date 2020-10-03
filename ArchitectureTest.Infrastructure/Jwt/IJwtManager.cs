using ArchitectureTest.Infrastructure.Jwt.Models;

namespace ArchitectureTest.Infrastructure.Jwt {
	public interface IJwtManager {
		JsonWebToken GenerateToken(JwtUser user);
		string GenerateRefreshToken();
		JwtUser ReadToken(string token, bool validateLifeTime);
		JsonWebToken ExchangeRefreshToken(string accessToken, string refreshToken);
	}
}
