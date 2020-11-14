using ArchitectureTest.Infrastructure.Jwt.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ArchitectureTest.Infrastructure.Jwt {
	public class JwtManager : IJwtManager {
		private const int TokenTTLMinutes = 1;
		private const int RefreshTokenTTLHours = 720;

		private readonly JwtSecurityTokenHandler tokenHandler;
		private readonly TokenValidationParameters tokenValidationParameters;

		public JwtManager(TokenValidationParameters tokenValidationParameters) {
			tokenHandler = new JwtSecurityTokenHandler();
			this.tokenValidationParameters = tokenValidationParameters;
		}
		public JsonWebToken GenerateToken(JwtUser user) {
			var tokenDescriptor = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim(ClaimTypes.Name, user.Name)
				}),
				Expires = DateTime.UtcNow.AddMinutes(TokenTTLMinutes),
				Issuer = tokenValidationParameters.ValidIssuer,
				Audience = tokenValidationParameters.ValidAudience,
				SigningCredentials = new SigningCredentials(tokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return new JsonWebToken {
				Token = tokenHandler.WriteToken(token),
				Email = user.Email,
				RefreshToken = GenerateRefreshToken(),
				UserId = user.Id,
				ExpiresIn = TokenTTLMinutes * 60
			};
		}
		public string GenerateRefreshToken() {
			var randomNumber = new byte[32];
			using (var rng = RandomNumberGenerator.Create()) {
				rng.GetBytes(randomNumber);
				return Convert.ToBase64String(randomNumber);
			}
		}
		public ClaimsPrincipal ReadToken(string token, bool validateLifeTime) {
			tokenValidationParameters.ValidateLifetime = validateLifeTime;
			var claims = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
			return claims;
		}

		public JwtWithClaims ExchangeRefreshToken(string accessToken, string refreshToken) {
			// validate refreshToken in DB
			var tokenExistsInBD = true;
			if (tokenExistsInBD) {
				ClaimsPrincipal oldClaims = ReadToken(accessToken, false);
				var user = new JwtUser {
					Email = oldClaims.FindFirst(ClaimTypes.Email)?.Value,
					Id = long.Parse(oldClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value),
					Name = oldClaims.FindFirst(ClaimTypes.Name)?.Value
				};
				return new JwtWithClaims {
					JsonWebToken = GenerateToken(user),
					Claims = oldClaims
				};
			}
			return null;
		}
	}
}
