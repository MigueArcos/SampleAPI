using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.TestUtils;
using ArchitectureTest.Web.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ArchitectureTest.Web.Tests;

public class GlobalHttpExceptionHandlerTests
{
    private readonly ILogger<GlobalHttpExceptionHandler> _mockLogger;
    private readonly GlobalHttpExceptionHandler _systemUnderTest;

    public GlobalHttpExceptionHandlerTests()
    {
        _mockLogger = Substitute.For<ILogger<GlobalHttpExceptionHandler>>();

        _systemUnderTest = new GlobalHttpExceptionHandler(_mockLogger);
    }

    [Theory]
    [ClassData(typeof(AllErrorCodesInputData))]
    public async Task TryHandlerAsync_WhenUserIsLoggedIn_ShouldLogExceptionAndUserInfo(string errorCode)
    {
        // Arrange
        var exceptionToLog = new Exception(errorCode);
        var httpContext = SetupHttpContext(false);
    
        // Act
        bool result = await _systemUnderTest.TryHandleAsync(httpContext, exceptionToLog, default);

        // Assert
        result.Should().BeTrue();
        _mockLogger.ReceivedWithAnyArgs(1).LogError(
            exceptionToLog,
            default,
            StubData.UserId,
            StubData.Email
        );
        var expectedErrorInfo = HttpResponses.TryGetErrorInfo(exceptionToLog.Message);
        httpContext.Response.StatusCode.Should().Be(expectedErrorInfo.HttpStatusCode);
    }

    [Theory]
    [ClassData(typeof(AllErrorCodesInputData))]
    public async Task TryHandlerAsync_WhenUserIsNotLoggedIn_ShouldLogOnlyException(string errorCode)
    {
        // Arrange
        var exceptionToLog = new Exception(errorCode);
        var httpContext = SetupHttpContext(true);
    
        // Act
        bool result = await _systemUnderTest.TryHandleAsync(httpContext, exceptionToLog, default);

        // Assert
        result.Should().BeTrue();
        _mockLogger.ReceivedWithAnyArgs(1).LogError(
            exceptionToLog,
            default
        );
        var expectedErrorInfo = HttpResponses.TryGetErrorInfo(exceptionToLog.Message);
        httpContext.Response.StatusCode.Should().Be(expectedErrorInfo.HttpStatusCode);
    }

    internal class AllErrorCodesInputData : TheoryData<string>
    {
        public AllErrorCodesInputData()
        {
            var allErrorCodes = typeof(ErrorCodes)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => (string)x.GetRawConstantValue()!)
                .ToList();
            foreach (var errorCode in allErrorCodes) {
                Add(errorCode);
            }
        }
    }

    private HttpContext SetupHttpContext(bool isAnonymous)
    {
        var httpContext = new DefaultHttpContext();

        if (!isAnonymous)
        {
            var userClaims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, StubData.UserId),
                new Claim(ClaimTypes.Email, StubData.Email),
                new Claim(ClaimTypes.Name, StubData.UserName)
            };
            var identity = new ClaimsIdentity(userClaims, "TestAuthType");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        return httpContext;
    }
}
