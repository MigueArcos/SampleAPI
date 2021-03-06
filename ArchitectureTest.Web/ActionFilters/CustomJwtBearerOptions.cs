﻿using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;
using ArchitectureTest.Domain.Models.StatusCodes;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using ArchitectureTest.Infrastructure.Extensions;
using ArchitectureTest.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArchitectureTest.Domain.ServiceLayer.JwtManager;

namespace ArchitectureTest.Web.ActionFilters {
	public class CustomJwtBearerEvents : JwtBearerEvents {
		private readonly IJwtManager jwtManager;
        private readonly IRepository<UserToken> tokensRepository;
        public CustomJwtBearerEvents(IJwtManager jwtManager, IUnitOfWork unitOfWork) {
			this.jwtManager = jwtManager;
            this.tokensRepository = unitOfWork.Repository<UserToken>();
		}
		public override Task MessageReceived(MessageReceivedContext context) {
			GetTokensFromRequestContext(context.HttpContext.Request, out string accessToken, out string refreshToken);
			context.Token = accessToken;
			return Task.CompletedTask;
		}

		public override Task Challenge(JwtBearerChallengeContext context) {
			if (context.AuthenticateFailure != null) {
                WriteExceptionToHttpResponse(context.HttpContext.Response, ErrorStatusCode.AuthorizationFailed);
				context.HandleResponse();
			}
			return Task.CompletedTask;
		}
		// AuthenticationFailed, try again using the refreshToken
		public override async Task AuthenticationFailed(AuthenticationFailedContext context) {
			try {
				GetTokensFromRequestContext(context.HttpContext.Request, out string token, out string refreshToken);
				if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refreshToken)) {
                    // validate refreshToken in DB
                    var refreshTokenSearch = await tokensRepository.Get(t => t.Token == refreshToken);
                    if (refreshTokenSearch == null || refreshTokenSearch.Count == 0) {
                        WriteExceptionToHttpResponse(context.HttpContext.Response, ErrorStatusCode.RefreshTokenExpired);
                        throw ErrorStatusCode.RefreshTokenExpired;
                    }
                    var (claims, jwtUser) = jwtManager.ReadToken(token, false);
                    var newToken = jwtManager.GenerateToken(jwtUser);
                    // Delete previous token from database
                    await tokensRepository.DeleteById(refreshTokenSearch[0].Id);
                    // Create a new token in Database
                    await tokensRepository.Post(new UserToken {
                        UserId = newToken.UserId,
                        Token = newToken.RefreshToken,
                        TokenTypeId = (long)Data.Enums.TokenType.RefreshToken,
                        ExpiryTime = DateTime.Now.AddSeconds(jwtManager.RefreshTokenTTLSeconds)
                    });
                    context.Principal = claims;
					// if there was a cookie, then set again the cookie with the new value
					if (!string.IsNullOrEmpty(context.HttpContext.Request.Cookies[AppConstants.SessionCookie])) {
						context.HttpContext.SetCookie(AppConstants.SessionCookie, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string> {
							[AppConstants.Token] = newToken.Token,
							[AppConstants.RefreshToken] = newToken.RefreshToken
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

        private void WriteExceptionToHttpResponse(HttpResponse httpResponseContext, ErrorStatusCode exception) {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(exception.Detail);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            httpResponseContext.StatusCode = exception.HttpStatusCode;
            httpResponseContext.Headers.Add("Content-Type", "application/json");
            httpResponseContext.Body.Write(bytes, 0, bytes.Length);
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
