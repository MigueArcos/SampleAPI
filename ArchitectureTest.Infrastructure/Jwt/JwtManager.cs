using ArchitectureTest.Infrastructure.AppConfiguration;
using ArchitectureTest.Infrastructure.Helpers;
using ArchitectureTest.Infrastructure.Jwt.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ArchitectureTest.Infrastructure.Jwt {
	public class JwtManager : IJwtManager {
		private readonly JwtConfiguration jwtConfiguration;
		private const int TokenTTLMinutes = 1;
		private const int RefreshTokenTTLHours = 720;

		private SymmetricSecurityKey securityKey;
		private JwtSecurityTokenHandler tokenHandler;

		public JwtManager(ConfigData configData) {
			jwtConfiguration = configData.Jwt;

			securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfiguration.Secret));
			tokenHandler = new JwtSecurityTokenHandler();
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
				Issuer = jwtConfiguration.Issuer,
				Audience = jwtConfiguration.Audience,
				SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
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
			var claims = tokenHandler.ValidateToken(token, new TokenValidationParameters {
				ValidateIssuerSigningKey = true,
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidIssuer = jwtConfiguration.Issuer,
				ValidAudience = jwtConfiguration.Audience,
				IssuerSigningKey = securityKey,
				ValidateLifetime = validateLifeTime
			}, out SecurityToken validatedToken);
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
