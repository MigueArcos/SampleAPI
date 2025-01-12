using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
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

namespace ArchitectureTest.Web.Tests.Authentication;

public class CustomJwtBearerEventsTests
{
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly ILogger<CustomJwtBearerEvents> _mockLogger;
    private readonly IAuthService _mockAuthService;


    private readonly CustomJwtBearerEvents _systemUnderTest;


    public CustomJwtBearerEventsTests()
    {
        _mockAuthService = Substitute.For<IAuthService>();
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _mockLogger = Substitute.For<ILogger<CustomJwtBearerEvents>>();

        var userClaims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, StubData.UserId.ToString()),
            new Claim(ClaimTypes.Email, StubData.Email),
            new Claim(ClaimTypes.Name, StubData.UserName),
        };
        var identity = new ClaimsIdentity(userClaims, "TestAuthType");
    
        var httpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(identity)
        };

        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        _systemUnderTest = new CustomJwtBearerEvents(
            _mockLogger, _mockAuthService
        );
    }
    public class TestAuthScheme : IAuthenticationHandler
    {
        public Task<AuthenticateResult> AuthenticateAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task ChallengeAsync(AuthenticationProperties? properties)
        {
            throw new System.NotImplementedException();
        }

        public Task ForbidAsync(AuthenticationProperties? properties)
        {
            throw new System.NotImplementedException();
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            throw new System.NotImplementedException();
        }
    }
    public class AnonymousAttr : IAllowAnonymous {

    }

    public class DictionaryRequestCookieCollection : Dictionary<string, string>, IRequestCookieCollection
    {
        public new ICollection<string> Keys => base.Keys;

        public new string this[string key]{
            get {
                TryGetValue(key, out var value);
                return value!;
            }
            set => base[key] = value;
        }
    }

    [Fact]
    public async Task MessageReceived_WhenIsAnonymous_ShouldReturnCompletedTask()
    {
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: true);
        var bearerOptions = new JwtBearerOptions();
        var authScheme = new AuthenticationScheme("schemaName", "displayName", typeof(TestAuthScheme));
        var inputData = new MessageReceivedContext(httpContext, authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.MessageReceived(inputData);

        // Assert
        inputData.Token.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task MessageReceived_WhenIsNotAnonymousAndHeaderContainsToken_ShouldReturnCompletedTask()
    {
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: false, tokenInHeader: true);
        var bearerOptions = new JwtBearerOptions();
        var authScheme = new AuthenticationScheme("schemaName", "displayName", typeof(TestAuthScheme));
        var inputData = new MessageReceivedContext(httpContext, authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.MessageReceived(inputData);

        // Assert
        inputData.Token.Should().Be(StubData.JwtToken);
    }

    [Fact]
    public async Task MessageReceived_WhenIsNotAnonymousAndCookieContainsToken_ShouldReturnCompletedTask()
    {
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: false, tokenInCookie: true);
        var bearerOptions = new JwtBearerOptions();
        var authScheme = new AuthenticationScheme("schemaName", "displayName", typeof(TestAuthScheme));
        var inputData = new MessageReceivedContext(httpContext, authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.MessageReceived(inputData);

        // Assert
        inputData.Token.Should().Be(StubData.JwtToken);
    }

    [Fact]
    public async Task MessageReceived_WhenIsNotAnonymousAndCookieIsMalformed_ShouldReturnCompletedTaskWithEmptyToken()
    {
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: false, tokenInCookie: true, valueForCookie: "{badJson");
        var bearerOptions = new JwtBearerOptions();
        var authScheme = new AuthenticationScheme("schemaName", "displayName", typeof(TestAuthScheme));
        var inputData = new MessageReceivedContext(httpContext, authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.MessageReceived(inputData);

        // Assert
        inputData.Token.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AuthenticationFailed_WhenRefreshTokenExistInCookie_ShouldLoginSuccessfully()
    {
        // Arrange
        var httpContext = SetupHttpContext(isAnonymous: false, tokenInCookie: true);
        var bearerOptions = new JwtBearerOptions();
        var authScheme = new AuthenticationScheme("schemaName", "displayName", typeof(TestAuthScheme));
        var inputData = new AuthenticationFailedContext(httpContext, authScheme, bearerOptions);
    
        // Act
        await _systemUnderTest.AuthenticationFailed(inputData);

        // Assert
    }

    private HttpContext SetupHttpContext(
        bool isAnonymous = false, bool tokenInHeader = false, bool tokenInCookie = false, string? valueForCookie = null
    ){
        
        static Task requestDelegate(HttpContext context) => Task.CompletedTask;

        var httpContext = new DefaultHttpContext();
        var anonymous = new AnonymousAttr();
        var metadata = new EndpointMetadataCollection(isAnonymous ? anonymous : null!);

        var endpoint = new Endpoint(requestDelegate, metadata, "endpoint");
        httpContext.SetEndpoint(endpoint);
        httpContext.Request.Path = "/api/v1";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Scheme = "https";

        if (tokenInHeader)
            httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {StubData.JwtToken}";
        
        var cookies = new DictionaryRequestCookieCollection();
        httpContext.Request.Cookies = cookies;

        if (tokenInCookie)
            cookies.Add(
                AppConstants.SessionCookie,
                valueForCookie ?? JsonSerializer.Serialize(new Dictionary<string, string> {
                    [AppConstants.Token] = StubData.JwtToken,
                    [AppConstants.RefreshToken] = StubData.RefreshToken
                })
            );

        return httpContext;
    }
}
