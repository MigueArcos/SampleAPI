﻿using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using ArchitectureTest.Web.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using System.Text;
using ArchitectureTest.Domain.ServiceLayer.JwtManager;
using ArchitectureTest.Web.Extensions;
using System.Text.Json;
using ArchitectureTest.Domain.Models.Enums;
using ArchitectureTest.Web.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace ArchitectureTest.Web.Authentication;

public class CustomJwtBearerEvents : JwtBearerEvents {
	private readonly IJwtManager _jwtManager;
    private readonly IRepository<long, UserToken> _tokensRepository;
	private readonly ILogger<CustomJwtBearerEvents> _logger;

    public CustomJwtBearerEvents(IJwtManager jwtManager, IUnitOfWork unitOfWork, ILogger<CustomJwtBearerEvents> logger) {
		_jwtManager = jwtManager;
        _tokensRepository = unitOfWork.Repository<UserToken>();
		_logger = logger;
	}

	public override Task MessageReceived(MessageReceivedContext context) {
		var endpoint = context.HttpContext.GetEndpoint();
		bool isAnonymous = endpoint == null || endpoint.Metadata?.GetMetadata<IAllowAnonymous>() != null;
		if (!isAnonymous){
			GetTokensFromRequestContext(context.HttpContext.Request, out string? accessToken, out string? _);
			context.Token = accessToken;
		}
		return Task.CompletedTask;
	}

	public override async Task Challenge(JwtBearerChallengeContext context) {
		if (context.AuthenticateFailure != null) {
			await WriteExceptionToHttpResponse(context.HttpContext.Response, new Exception(ErrorCodes.AuthorizationFailed));
			context.HandleResponse();
		}
	}
	
	// AuthenticationFailed, try again using the refreshToken
	public override async Task AuthenticationFailed(AuthenticationFailedContext context) {
		try {
			GetTokensFromRequestContext(context.HttpContext.Request, out string? token, out string? refreshToken);
			if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refreshToken)) {
				// validate refreshToken in DB
				var refreshTokenSearch = await _tokensRepository.FindSingle(t => t.Token == refreshToken);
				if (refreshTokenSearch == null) {
					await WriteExceptionToHttpResponse(context.HttpContext.Response, new Exception(ErrorCodes.RefreshTokenExpired));
					throw new Exception(ErrorCodes.RefreshTokenExpired);
				}
				var (claims, jwtUser) = _jwtManager.ReadToken(token, false);
				var newToken = _jwtManager.GenerateToken(jwtUser);
				// Delete previous token from database
				await _tokensRepository.DeleteById(refreshTokenSearch.Id);
				// Create a new token in Database
				await _tokensRepository.Add(new UserToken {
					UserId = newToken.UserId,
					Token = newToken.RefreshToken,
					TokenTypeId = (long)Data.Enums.TokenType.RefreshToken,
					ExpiryTime = DateTime.Now.AddSeconds(_jwtManager.RefreshTokenTTLSeconds)
				});
				context.Principal = claims;
				// if there was a cookie, then set again the cookie with the new value
				if (!string.IsNullOrEmpty(context.HttpContext.Request.Cookies[AppConstants.SessionCookie])) {
					context.HttpContext.SetCookie(
						AppConstants.SessionCookie,
						JsonSerializer.Serialize(new Dictionary<string, string> {
							[AppConstants.Token] = newToken.Token,
							[AppConstants.RefreshToken] = newToken.RefreshToken
						})
					);
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

    private async Task WriteExceptionToHttpResponse(HttpResponse httpResponseContext, Exception exception) {
		var errorInfo = BaseController.GetHttpErrorFromException(exception, _logger);
        var json = JsonSerializer.Serialize(errorInfo);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        httpResponseContext.StatusCode = errorInfo.HttpStatusCode;
        httpResponseContext.Headers.Append("Content-Type", "application/json");
        await httpResponseContext.Body.WriteAsync(bytes, 0, bytes.Length);
    }

	private bool GetTokensFromRequestContext(HttpRequest requestContext, out string? accessToken, out string? refreshToken) {
		const string defaultBearerValue = "Bearer ";
		try {
			var cookieObj = JsonSerializer.Deserialize<Dictionary<string, string>>(
				requestContext.Cookies[AppConstants.SessionCookie] ?? "{}"
			);
			var authorizationHeader = requestContext.Headers[HeaderNames.Authorization].ToString();
			if (!string.IsNullOrEmpty(authorizationHeader)) {
				var array = authorizationHeader.Split(defaultBearerValue);
				accessToken = array.Length > 1 ? array[1] : defaultBearerValue;
				var refreshTokenHeader = requestContext.Headers[AppConstants.RefreshTokenHeader].ToString();
				refreshToken = string.IsNullOrEmpty(refreshTokenHeader) ? string.Empty : refreshTokenHeader;
			}
			else if (cookieObj is not null && cookieObj.Count > 0) {
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