using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ArchitectureTest.TestUtils;
using ArchitectureTest.Web.Authentication;
using ArchitectureTest.Domain.Services.Application.AuthService;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using FluentAssertions;
using Microsoft.Net.Http.Headers;
using ArchitectureTest.Web.Configuration;
using System.Text.Json;
using ArchitectureTest.Domain.Errors;
using System;

namespace ArchitectureTest.Web.Tests.Authentication;

public class CustomJwtBearerEventsTests
{
    private readonly ILogger<CustomJwtBearerEvents> _mockLogger;
    private readonly IAuthService _mockAuthService;
    private readonly AuthenticationScheme _authScheme = new("schemaName", "displayName", typeof(IAuthenticationHandler));

    private readonly CustomJwtBearerEvents _systemUnderTest;

    public CustomJwtBearerEventsTests()
    {
        _mockAuthService = Substitute.For<IAuthService>();
        _mockLogger = Substitute.For<ILogger<CustomJwtBearerEvents>>();

        _systemUnderTest = new CustomJwtBearerEvents(
            _mockLogger, _mockAuthService
        );
    }

    [Fact]
    public async Task MessageReceived_WhenIsAnonymous_ShouldReturnCompletedTask()
    {
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: true);
        var bearerOptions = new JwtBearerOptions();
        var inputData = new MessageReceivedContext(httpContext, _authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.MessageReceived(inputData);

        // Assert
        inputData.Token.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task MessageReceived_WhenIsNotAnonymousAndHeaderContainsToken_ShouldReturnCompletedTask()
    {
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: false, tokensInHeaders: true);
        var bearerOptions = new JwtBearerOptions();
        var inputData = new MessageReceivedContext(httpContext, _authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.MessageReceived(inputData);

        // Assert
        inputData.Token.Should().Be(StubData.JwtToken);
    }

    [Fact]
    public async Task MessageReceived_WhenIsNotAnonymousAndCookieContainsToken_ShouldReturnCompletedTask()
    {
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: false, tokensInCookie: true);
        var bearerOptions = new JwtBearerOptions();
        var inputData = new MessageReceivedContext(httpContext, _authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.MessageReceived(inputData);

        // Assert
        inputData.Token.Should().Be(StubData.JwtToken);
    }

    [Fact]
    public async Task MessageReceived_WhenIsNotAnonymousAndCookieIsMalformed_ShouldReturnCompletedTaskWithEmptyToken()
    {
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: false, tokensInCookie: true, valueForCookie: "{badJson");
        var bearerOptions = new JwtBearerOptions();
        var inputData = new MessageReceivedContext(httpContext, _authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.MessageReceived(inputData);

        // Assert
        inputData.Token.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Challenge_WhenAuthFailureIsNotNull_ShouldLogException()
    {
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: false, tokensInCookie: true, valueForCookie: "{badJson");
        var bearerOptions = new JwtBearerOptions();
        var thrownException = new Exception(ErrorCodes.UnknownError);
        var inputData = new JwtBearerChallengeContext(httpContext, _authScheme, bearerOptions, default!){
            AuthenticateFailure = thrownException,
            Error = ErrorCodes.UnknownError,
            ErrorDescription = ErrorMessages.DefaultErrorMessageForExceptions
        };
    
        // Act
        await _systemUnderTest.Challenge(inputData);

        // Assert
        _mockLogger.ReceivedWithAnyArgs(1).LogError(
            thrownException,
            default,
            inputData.Error,
            inputData.ErrorDescription
        );
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task AuthenticationFailed_WhenRefreshTokenExistsAndIsValid_ShouldRefreshLoginSuccessfully(
        bool tokensInCookie, bool tokensInHeaders
    ){
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: false, tokensInCookie: tokensInCookie, tokensInHeaders: tokensInHeaders);
        var bearerOptions = new JwtBearerOptions();
        var jwt = TestDataBuilders.BuildJwt();
        var claimsPrincipal = TestDataBuilders.BuildClaimsPrincipal();
        _mockAuthService.ExchangeOldTokensForNewToken(StubData.JwtToken, StubData.RefreshToken).Returns((jwt, claimsPrincipal));
        var inputData = new AuthenticationFailedContext(httpContext, _authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.AuthenticationFailed(inputData);

        // Assert
        inputData.Principal.Should().Be(claimsPrincipal);
        await _mockAuthService.Received(1).ExchangeOldTokensForNewToken(StubData.JwtToken, StubData.RefreshToken);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task AuthenticationFailed_WhenAuthServiceFails_ShouldNotRefreshLogin(
        bool tokensInCookie, bool tokensInHeaders
    ){
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: false, tokensInCookie: tokensInCookie, tokensInHeaders: tokensInHeaders);
        var bearerOptions = new JwtBearerOptions();
        _mockAuthService.ExchangeOldTokensForNewToken(StubData.JwtToken, StubData.RefreshToken)
            .Returns(new AppError(ErrorCodes.RefreshTokenExpired));
        var inputData = new AuthenticationFailedContext(httpContext, _authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.AuthenticationFailed(inputData);

        // Assert
        var expectedErrorInfo = HttpResponses.TryGetErrorInfo(ErrorCodes.RefreshTokenExpired);
        inputData.Principal.Should().BeNull();
        httpContext.Response.StatusCode.Should().Be(expectedErrorInfo.HttpStatusCode);

        await _mockAuthService.Received(1).ExchangeOldTokensForNewToken(StubData.JwtToken, StubData.RefreshToken);
    }

    private HttpContext SetupHttpContext(
        bool isAnonymous = false, bool tokensInHeaders = false, bool tokensInCookie = false, string? valueForCookie = null
    ){
        
        static Task requestDelegate(HttpContext context) => Task.CompletedTask;

        var httpContext = new DefaultHttpContext();
        var anonymousAttribute = Substitute.For<IAllowAnonymous>();
        var metadata = new EndpointMetadataCollection(isAnonymous ? anonymousAttribute : null!);

        var endpoint = new Endpoint(requestDelegate, metadata, "endpoint");
        httpContext.SetEndpoint(endpoint);
        httpContext.Request.Path = "/api/v1";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Scheme = "https";

        if (tokensInHeaders)
        {
            httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {StubData.JwtToken}";
            httpContext.Request.Headers[AppConstants.RefreshTokenHeader] = StubData.RefreshToken;
        }
        
        var cookiesMock = Substitute.For<IRequestCookieCollection>();
        httpContext.Request.Cookies = cookiesMock;

        if (tokensInCookie)
            cookiesMock[AppConstants.SessionCookie].Returns(valueForCookie ?? JsonSerializer.Serialize(
                new Dictionary<string, string> {
                    [AppConstants.Token] = StubData.JwtToken,
                    [AppConstants.RefreshToken] = StubData.RefreshToken
                })
            );

        return httpContext;
    }
}
