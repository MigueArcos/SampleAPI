using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Web.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using System.Text;
using ArchitectureTest.Web.Extensions;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using ArchitectureTest.Domain.Services.Infrastructure;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services.Application.AuthService;

namespace ArchitectureTest.Web.Authentication;

public class CustomJwtBearerEvents : JwtBearerEvents {
    private readonly IJwtManager _jwtManager;
    private readonly IAuthService _authService;
    private readonly IRepository<long, UserToken> _tokensRepository;
    private readonly ILogger<CustomJwtBearerEvents> _logger;

    public CustomJwtBearerEvents(
        IJwtManager jwtManager, IUnitOfWork unitOfWork, ILogger<CustomJwtBearerEvents> logger, IAuthService authService
    ) {
        _jwtManager = jwtManager;
        _tokensRepository = unitOfWork.Repository<UserToken>();
        _logger = logger;
        _authService = authService;
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
            await WriteErrorToHttpResponse(context.HttpContext.Response, ErrorCodes.AuthorizationFailed).ConfigureAwait(false);
            context.HandleResponse();
        }
    }
    
    // AuthenticationFailed, try again using the refreshToken
    public override async Task AuthenticationFailed(AuthenticationFailedContext context) {
        try {
            GetTokensFromRequestContext(context.HttpContext.Request, out string? token, out string? refreshToken);
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refreshToken)) {
                var tokenExchangeResult = await _authService.ExchangeOldTokensForNewToken(token, refreshToken)
                    .ConfigureAwait(false);

                if (tokenExchangeResult.Error != null){
                    await WriteErrorToHttpResponse(context.HttpContext.Response, tokenExchangeResult.Error.Code)
                        .ConfigureAwait(false);
                    return;
                }

                var (newToken, claims) = tokenExchangeResult.Value;

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

    private async Task WriteErrorToHttpResponse(HttpResponse httpResponseContext, string errorCode) {
        var errorInfo = HttpResponses.TryGetErrorInfo(errorCode, message => _logger.LogError(message));
        var json = JsonSerializer.Serialize(errorInfo);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        httpResponseContext.StatusCode = errorInfo!.HttpStatusCode;
        httpResponseContext.Headers.Append("Content-Type", "application/json");
        await httpResponseContext.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
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
