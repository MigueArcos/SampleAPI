using ArchitectureTest.Infrastructure.AppConfiguration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ArchitectureTest.Infrastructure.Helpers {
	public class JwtTokenManager {
		private readonly JwtConfiguration jwtConfiguration;
		public JwtTokenManager(JwtConfiguration jwtConfiguration) {
			this.jwtConfiguration = jwtConfiguration;
		}
		public string GenerateToken(long userId) {
			var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfiguration.Secret));

			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenDescriptor = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(AppConstants.UserId, userId.ToString()),
				}),
				Expires = DateTime.UtcNow.AddDays(7),
				Issuer = jwtConfiguration.Issuer,
				Audience = jwtConfiguration.Audience,
				SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
		public bool TokenIsValid(string token) {
			var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfiguration.Secret));

			var tokenHandler = new JwtSecurityTokenHandler();
			try {
				tokenHandler.ValidateToken(token, new TokenValidationParameters {
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidIssuer = jwtConfiguration.Issuer,
					ValidAudience = jwtConfiguration.Audience,
					IssuerSigningKey = securityKey
				}, out SecurityToken validatedToken);
			}
			catch {
				return false;
			}
			return true;
		}

		public string GetClaim(string token, string claimType) {
			var tokenHandler = new JwtSecurityTokenHandler();
			var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

			var stringClaimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
			return stringClaimValue;
		}
	}
}
