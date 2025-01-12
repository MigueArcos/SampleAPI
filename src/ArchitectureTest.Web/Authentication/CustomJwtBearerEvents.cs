using ArchitectureTest.Web.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using System.Text;
using ArchitectureTest.Web.Extensions;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using ArchitectureTest.Domain.Services.Application.AuthService;

namespace ArchitectureTest.Web.Authentication;

public class CustomJwtBearerEvents : JwtBearerEvents {
    private readonly IAuthService _authService;
    private readonly ILogger<CustomJwtBearerEvents> _logger;

    public CustomJwtBearerEvents(ILogger<CustomJwtBearerEvents> logger, IAuthService authService) 
    {
        _logger = logger;
        _authService = authService;
    }

    public override Task MessageReceived(MessageReceivedContext context) {
        var endpoint = context.HttpContext.GetEndpoint();
        bool isAnonymous = endpoint == null || endpoint.Metadata?.GetMetadata<IAllowAnonymous>() != null;
        if (!isAnonymous){
            var (accessToken, _) = GetTokensFromRequestContext(context.HttpContext.Request);
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }

    public override Task Challenge(JwtBearerChallengeContext context) {
        if (context.AuthenticateFailure != null)
        {
            _logger.LogError(JsonSerializer.Serialize(new {
                AuthFailure = context.AuthenticateFailure.Message,
                context.Error,
                context.ErrorDescription
            }));
        }
        return base.Challenge(context);
    }
    
    // AuthenticationFailed, try again using the refreshToken
    public override async Task AuthenticationFailed(AuthenticationFailedContext context) {
        var (token, refreshToken) = GetTokensFromRequestContext(context.HttpContext.Request);

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
                context.HttpContext.SetResponseCookie(
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

    public override Task TokenValidated(TokenValidatedContext context) {
        return Task.CompletedTask;
    }

    private async Task WriteErrorToHttpResponse(HttpResponse httpResponseContext, string errorCode) {
        var errorInfo = HttpResponses.TryGetErrorInfo(errorCode, message => _logger.LogError(message))!;
        var json = JsonSerializer.Serialize(errorInfo);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        httpResponseContext.StatusCode = errorInfo.HttpStatusCode;
        httpResponseContext.Headers.Append("Content-Type", "application/json");
        await httpResponseContext.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
    }

    private (string? AccessToken, string? RefreshToken) GetTokensFromRequestContext(HttpRequest requestContext)
    {
        try {
            var (accessToken, refreshToken) = GetTokensFromHeaders(requestContext);

            if (string.IsNullOrEmpty(accessToken))
                (accessToken, refreshToken) = GetTokensFromCookies(requestContext);
            
            return (accessToken, refreshToken);
        }
        catch {
            return (string.Empty, string.Empty);
        }
    }

    private (string? AccessToken, string? RefreshToken) GetTokensFromHeaders(HttpRequest requestContext)
    {
        const string defaultBearerValue = "Bearer ";
        string accessToken = string.Empty,
               refreshToken = string.Empty;
        var authorizationHeader = requestContext.Headers[HeaderNames.Authorization].ToString();
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            accessToken = authorizationHeader.Replace(defaultBearerValue, string.Empty);
            var refreshTokenHeader = requestContext.Headers[AppConstants.RefreshTokenHeader].ToString();
            refreshToken = string.IsNullOrEmpty(refreshTokenHeader) ? string.Empty : refreshTokenHeader;
        }
        return (accessToken, refreshToken);
    }

    private (string? AccessToken, string? RefreshToken) GetTokensFromCookies(HttpRequest requestContext)
    {
        var cookieDict = JsonSerializer.Deserialize<Dictionary<string, string>>(
            requestContext.Cookies[AppConstants.SessionCookie] ?? "{}"
        );
        string? accessToken = null,
                refreshToken = null;
        if (cookieDict is not null && cookieDict.Count > 0)
        {
            cookieDict.TryGetValue(AppConstants.Token, out accessToken);
            cookieDict.TryGetValue(AppConstants.RefreshToken, out refreshToken);
        }
        return (accessToken, refreshToken);
    }
}
