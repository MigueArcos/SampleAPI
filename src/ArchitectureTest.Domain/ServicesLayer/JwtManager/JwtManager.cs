using ArchitectureTest.Domain.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ArchitectureTest.Domain.ServiceLayer.JwtManager;

public class JwtManager : IJwtManager {
	private readonly JwtSecurityTokenHandler _tokenHandler;
	private readonly TokenValidationParameters _tokenValidationParameters;

    public int TokenTTLSeconds => 3600;
    public int RefreshTokenTTLSeconds => 720 * TokenTTLSeconds; // 720 hours (30 days)

    public JwtManager(TokenValidationParameters tokenValidationParameters) {
		_tokenHandler = new JwtSecurityTokenHandler();
		_tokenValidationParameters = tokenValidationParameters;
	}

	public JsonWebToken GenerateToken(JwtUser user) {
		var tokenDescriptor = new SecurityTokenDescriptor {
			Subject = new ClaimsIdentity(new Claim[] {
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Name, user.Name)
			}),
			Expires = DateTime.UtcNow.AddSeconds(TokenTTLSeconds),
			Issuer = _tokenValidationParameters.ValidIssuer,
			Audience = _tokenValidationParameters.ValidAudience,
			SigningCredentials = new SigningCredentials(
				_tokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256Signature
			)
		};
		var token = _tokenHandler.CreateToken(tokenDescriptor);
		var refreshToken = GenerateRefreshToken();
		return new JsonWebToken {
			Token = _tokenHandler.WriteToken(token),
			Email = user.Email,
			RefreshToken = refreshToken,
			UserId = user.Id,
			ExpiresIn = TokenTTLSeconds
		};
	}

	public (ClaimsPrincipal claims, JwtUser jwtUser) ReadToken(string token, bool validateLifeTime) {
		_tokenValidationParameters.ValidateLifetime = validateLifeTime;
		var claims = _tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken validatedToken);
		var user = new JwtUser {
			Email = claims.FindFirst(ClaimTypes.Email)?.Value ?? "user@default.io",
			Id = long.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
			Name = claims.FindFirst(ClaimTypes.Name)?.Value ?? "Default"
		};
		return (claims, user);
	}

	private string GenerateRefreshToken() {
		var randomNumber = new byte[32];
		using (var rng = RandomNumberGenerator.Create()) {
			rng.GetBytes(randomNumber);
			return Convert.ToBase64String(randomNumber);
		}
	}
}
