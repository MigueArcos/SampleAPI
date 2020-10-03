using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Infrastructure.Extensions;
using ArchitectureTest.Infrastructure.Helpers;
using ArchitectureTest.Infrastructure.Jwt;
using ArchitectureTest.Infrastructure.Jwt.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.ActionFilters {
	public class CustomJwtBearerEvents: JwtBearerEvents {
		private readonly IJwtManager jwtManager;
		public CustomJwtBearerEvents(IJwtManager jwtManager){
			this.jwtManager = jwtManager;
		}
		public override Task MessageReceived(MessageReceivedContext context){
			var cookieObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(context.Request.Cookies[AppConstants.SessionCookie] ?? "{}");
			var authorizationHeader = context.Request.Headers["Authorization"].ToString();
			if (!string.IsNullOrEmpty(authorizationHeader)){
				var array = authorizationHeader.Split("Bearer ");
				context.Token = array.Length > 1 ? array[1] : null; 
			}
			else if (!string.IsNullOrEmpty(context.Request.Cookies[AppConstants.SessionCookie])){
				context.Token = cookieObj[AppConstants.Token];
			}
			if (!string.IsNullOrEmpty(context.Token)){
				try{
					var tokenResult = jwtManager.ReadToken(context.Token, true);
				} catch (Exception e){
					if (!string.IsNullOrEmpty(cookieObj[AppConstants.RefreshToken])){
						JsonWebToken newToken = jwtManager.ExchangeRefreshToken(cookieObj[AppConstants.Token], cookieObj[AppConstants.RefreshToken]);
						context.HttpContext.SetCookie(AppConstants.SessionCookie, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string> {
							[AppConstants.Token] = newToken.RefreshToken,
							[AppConstants.RefreshToken] = newToken.Token
						}));
						context.Token = newToken.Token;
					}
				}		
			}
			return Task.CompletedTask;
		}
		public override Task Challenge(JwtBearerChallengeContext context){
			if (context.AuthenticateFailure != null){
				var json = Newtonsoft.Json.JsonConvert.SerializeObject(ErrorStatusCode.AuthorizationFailed.StatusCode);
				byte[] bytes = Encoding.UTF8.GetBytes(json);
				context.HttpContext.Response.StatusCode = 401;
				context.HttpContext.Response.Headers.Add("Content-Type", "application/json");
				context.HttpContext.Response.Body.Write(bytes, 0, bytes.Length);
				context.HandleResponse();
			}
			return Task.CompletedTask;
		}
	}
}
