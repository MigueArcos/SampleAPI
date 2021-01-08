using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Contracts;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Repositories.BasicRepo;
using ArchitectureTest.Domain.StatusCodes;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Services {
	public class JwtManager : IJwtManager {
		private readonly JwtSecurityTokenHandler tokenHandler;
		private readonly TokenValidationParameters tokenValidationParameters;

        public int TokenTTLSeconds => 3600;

        public int RefreshTokenTTLSeconds => 720 * TokenTTLSeconds; // 720 hours (30 days)

        public JwtManager(TokenValidationParameters tokenValidationParameters, IRepository<UserToken> tokensRepository) {
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
				Expires = DateTime.UtcNow.AddSeconds(TokenTTLSeconds),
				Issuer = tokenValidationParameters.ValidIssuer,
				Audience = tokenValidationParameters.ValidAudience,
				SigningCredentials = new SigningCredentials(tokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			var refreshToken = GenerateRefreshToken();
			return new JsonWebToken {
				Token = tokenHandler.WriteToken(token),
				Email = user.Email,
				RefreshToken = refreshToken,
				UserId = user.Id,
				ExpiresIn = TokenTTLSeconds
			};
		}

		public JwtWithClaims ExchangeRefreshToken(string accessToken, string refreshToken) {
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
		public ClaimsPrincipal ReadToken(string token, bool validateLifeTime) {
			tokenValidationParameters.ValidateLifetime = validateLifeTime;
			var claims = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
			return claims;
		}
		private string GenerateRefreshToken() {
			var randomNumber = new byte[32];
			using (var rng = RandomNumberGenerator.Create()) {
				rng.GetBytes(randomNumber);
				return Convert.ToBase64String(randomNumber);
			}
		}
	}
}
