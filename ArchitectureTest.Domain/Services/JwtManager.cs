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
		private const int TokenTTLMinutes = 60;
		private const int RefreshTokenTTLHours = 720;

		private readonly JwtSecurityTokenHandler tokenHandler;
		private readonly TokenValidationParameters tokenValidationParameters;
		private readonly IRepository<UserToken> tokensRepository;
		public JwtManager(TokenValidationParameters tokenValidationParameters, IRepository<UserToken> tokensRepository) {
			tokenHandler = new JwtSecurityTokenHandler();
			this.tokenValidationParameters = tokenValidationParameters;
			this.tokensRepository = tokensRepository;
		}
		public async Task<JsonWebToken> GenerateToken(JwtUser user) {
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
			var refreshToken = GenerateRefreshToken();
			await tokensRepository.Post(new UserToken {
				UserId = user.Id,
				Token = refreshToken,
				TokenTypeId = (long)Data.Enums.TokenType.RefreshToken,
				ExpiryTime = DateTime.Now.AddHours(RefreshTokenTTLHours)
			});
			return new JsonWebToken {
				Token = tokenHandler.WriteToken(token),
				Email = user.Email,
				RefreshToken = refreshToken,
				UserId = user.Id,
				ExpiresIn = TokenTTLMinutes * 60
			};
		}

		public async Task<JwtWithClaims> ExchangeRefreshToken(string accessToken, string refreshToken) {
			// validate refreshToken in DB
			var tokenSearch = await tokensRepository.Get(t => t.Token == refreshToken);
			if (tokenSearch == null || tokenSearch.Count == 0) throw ErrorStatusCode.RefreshTokenExpired;
			var token = tokenSearch[0];
			ClaimsPrincipal oldClaims = ReadToken(accessToken, false);
			var user = new JwtUser {
				Email = oldClaims.FindFirst(ClaimTypes.Email)?.Value,
				Id = long.Parse(oldClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value),
				Name = oldClaims.FindFirst(ClaimTypes.Name)?.Value
			};
			await tokensRepository.DeleteById(token.Id);
			return new JwtWithClaims {
				JsonWebToken = await GenerateToken(user),
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
