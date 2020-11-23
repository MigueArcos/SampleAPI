using ArchitectureTest.Domain.Contracts;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Infrastructure.Extensions;
using ArchitectureTest.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.ActionFilters {
	public class CustomJwtBearerEvents : JwtBearerEvents {
		private readonly IJwtManager jwtManager;
		public CustomJwtBearerEvents(IJwtManager jwtManager) {
			this.jwtManager = jwtManager;
		}
		public override Task MessageReceived(MessageReceivedContext context) {
			GetTokensFromRequestContext(context.HttpContext.Request, out string accessToken, out string refreshToken);
			context.Token = accessToken;
			return Task.CompletedTask;
		}

		public override Task Challenge(JwtBearerChallengeContext context) {
			if (context.AuthenticateFailure != null) {
				var json = Newtonsoft.Json.JsonConvert.SerializeObject(ErrorStatusCode.AuthorizationFailed.StatusCode);
				byte[] bytes = Encoding.UTF8.GetBytes(json);
				context.HttpContext.Response.StatusCode = 401;
				context.HttpContext.Response.Headers.Add("Content-Type", "application/json");
				context.HttpContext.Response.Body.Write(bytes, 0, bytes.Length);
				context.HandleResponse();
			}
			return Task.CompletedTask;
		}
		// AuthenticationFailed, try again using the refreshToken
		public override async Task AuthenticationFailed(AuthenticationFailedContext context) {
			try {
				GetTokensFromRequestContext(context.HttpContext.Request, out string token, out string refreshToken);
				if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refreshToken)) {
					JwtWithClaims newToken = await jwtManager.ExchangeRefreshToken(token, refreshToken);
					context.Principal = newToken.Claims;
					// if there was a cookie, then set again the cookie with the new value
					if (!string.IsNullOrEmpty(context.HttpContext.Request.Cookies[AppConstants.SessionCookie])) {
						context.HttpContext.SetCookie(AppConstants.SessionCookie, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string> {
							[AppConstants.Token] = newToken.JsonWebToken.Token,
							[AppConstants.RefreshToken] = newToken.JsonWebToken.RefreshToken
						}));
					}
					// If everything goes ok set request principal (In this point authentication is done and ok)
					context.Success();
				}
			}
			catch {
				return;
			}
			return;
		}

		public override Task TokenValidated(TokenValidatedContext context) {
			return Task.CompletedTask;
		}

		private bool GetTokensFromRequestContext(HttpRequest requestContext, out string accessToken, out string refreshToken) {
			const string defaultBearerValue = "Bearer ";
			try {
				var cookieObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(requestContext.Cookies[AppConstants.SessionCookie] ?? "{}");
				var authorizationHeader = requestContext.Headers[HeaderNames.Authorization].ToString();
				if (!string.IsNullOrEmpty(authorizationHeader)) {
					var array = authorizationHeader.Split(defaultBearerValue);
					accessToken = array.Length > 1 ? array[1] : defaultBearerValue;
					var refreshTokenHeader = requestContext.Headers[AppConstants.RefreshTokenHeader].ToString();
					refreshToken = string.IsNullOrEmpty(refreshTokenHeader) ? string.Empty : refreshTokenHeader;
				}
				else if (!string.IsNullOrEmpty(requestContext.Cookies[AppConstants.SessionCookie])) {
					cookieObj.TryGetValue(AppConstants.Token, out accessToken);
					cookieObj.TryGetValue(AppConstants.RefreshToken, out refreshToken);
				}
				else {
					accessToken = defaultBearerValue;
					refreshToken = string.Empty;
				}
				return true;
			}
			catch {
				accessToken = defaultBearerValue;
				refreshToken = string.Empty;
				return false;
			}
		}
	}
}
