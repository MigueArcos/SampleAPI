using ArchitectureTest.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using ArchitectureTest.TestUtils;
using ArchitectureTest.Web.Configuration;
using Microsoft.Extensions.Logging;
using ArchitectureTest.Domain.Services.Application.AuthService;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models.Application;
using NSubstitute;
using ArchitectureTest.Domain.Entities;
using System;
using FluentAssertions;

namespace ArchitectureTest.Web.Tests.Controllers;

public class LoginControllerTest {
    private readonly IAuthService _mockAuthService;
    private readonly ILogger<LoginController> _mockLogger;
    private readonly LoginController _systemUnderTest;

    public LoginControllerTest() {
        _mockAuthService = Substitute.For<IAuthService>();
        _mockLogger = Substitute.For<ILogger<LoginController>>();
        _systemUnderTest = new LoginController(_mockAuthService, _mockLogger);
    }

     [Fact]
    public async Task SignIn_WhenEverythingIsOK_ShouldReturnJwt()
    {
        // Arrange
        var jwtMockResult = BuildJwt();
        var inputData = new SignInModel {
            Email = StubData.Email,
            Password = StubData.Password
        };
        _mockAuthService.SignIn(inputData).Returns(jwtMockResult);
    
        // Act
        var result = await _systemUnderTest.SignIn(inputData, false) as ObjectResult;

        // Assert
        await _mockAuthService.Received(1).SignIn(inputData);
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        result!.Value.Should().BeOfType<JsonWebToken>();
        StubData.JsonCompare(jwtMockResult, result.Value as JsonWebToken).Should().BeTrue();
    }

    [Fact]
    public async Task SignIn_WhenModelStateIsNotValid_ShouldReturnBadRequest()
    {
        // Arrange
        var inputData = new SignInModel {
            Email = "emailWrong",
            Password = string.Empty
        };
        _systemUnderTest.ModelState.AddModelError("Email", ErrorCodes.InvalidEmail);
        _systemUnderTest.ModelState.AddModelError("Password", ErrorCodes.InvalidPassword);

        // Act
        var result = await _systemUnderTest.SignIn(inputData, false) as ObjectResult;

        // Assert
        await _mockAuthService.DidNotReceiveWithAnyArgs().SignIn(default!);
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
        result.Should().NotBeNull();
        result!.Value.Should().BeOfType<BadRequestHttpErrorInfo>();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        (result!.Value as BadRequestHttpErrorInfo)!.ErrorCode.Should().Be(ErrorCodes.ValidationsFailed);
        (result!.Value as BadRequestHttpErrorInfo)!.Errors!.Select(e => e.ErrorCode)
            .Should().BeEquivalentTo([ErrorCodes.InvalidEmail, ErrorCodes.InvalidPassword]);
    }

    [Theory]
    [InlineData(ErrorCodes.UnknownError, 0)]
    [InlineData(ErrorCodes.EmailAlreadyInUse, 0)]
    [InlineData(ErrorCodes.AuthorizarionMissing, 0)]
    [InlineData(ErrorCodes.AuthorizationFailed, 0)]
    [InlineData("Exception Found!", 1)]
    public async Task SignIn_WhenAuthServiceFails_ReturnsError(string errorCode, int loggerCalls)
    {
        // Arrange
        var inputData = new SignInModel {
            Email = StubData.Email,
            Password = StubData.Password
        };

        _mockAuthService.SignIn(inputData).Returns(new AppError(errorCode));

        // Act
        var result = await _systemUnderTest.SignIn(inputData, false) as ObjectResult;

        // Assert
        await _mockAuthService.Received(1).SignIn(inputData);
        result.Should().NotBeNull();
        result!.Value.Should().BeOfType<HttpErrorInfo>();
        var expectedErrorInfo = HttpResponses.TryGetErrorInfo(errorCode);
        (result!.Value as HttpErrorInfo)!.ErrorCode.Should().Be(expectedErrorInfo.ErrorCode);
        result.StatusCode.Should().Be(expectedErrorInfo.HttpStatusCode);

        if (loggerCalls > 0)
            _mockLogger.Received(loggerCalls).LogError(errorCode);
        else
            _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Theory]
    [ClassData(typeof(SignInModelInputData))]
    public void SignIn_WhenModelStateIsNotValid_ValidationShouldNotPass(string email, string password)
    {
        // Arrange
        var inputData = new SignInModel {
            Email = email,
            Password = password
        };
        var context = new ValidationContext(inputData, null, null);
        var results = new List<ValidationResult>();

        // Act
        var isModelStateValid = Validator.TryValidateObject(inputData, context, results, true);

        // Assert
        isModelStateValid.Should().BeFalse();
        var possibleErrorMessages = new List<string> {
            ErrorCodes.InvalidEmail,
            ErrorCodes.InvalidPassword
        };
        var errorCodesFromValidation = results.Select(e => e.ErrorMessage);
        possibleErrorMessages.Intersect(errorCodesFromValidation).Should().NotBeEmpty();
    }

    [Fact]
    public void SignIn_WhenModelStateIsNotValid_ValidationShouldPass()
    {
        // Arrange
        var inputData = new SignInModel {
            Email = StubData.Email,
            Password = StubData.Password
        };
        var context = new ValidationContext(inputData, null, null);
        var results = new List<ValidationResult>();

        // Act
        var isModelStateValid = Validator.TryValidateObject(inputData, context, results, true);

        // Assert
        isModelStateValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SignUp_WhenEverythingIsOK_ShouldReturnJwt()
    {
        // Arrange
        var jwtMockResult = BuildJwt();
        var inputData = new SignUpModel {
            Email = StubData.Email,
            Password = StubData.Password,
            UserName = StubData.UserName
        };
        _mockAuthService.SignUp(inputData).Returns(jwtMockResult);
    
        // Act
        var result = await _systemUnderTest.SignUp(inputData, false) as ObjectResult;

        // Assert
        await _mockAuthService.Received(1).SignUp(inputData);
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        result!.Value.Should().BeOfType<JsonWebToken>();
        StubData.JsonCompare(jwtMockResult, result.Value as JsonWebToken).Should().BeTrue();
    }

    [Fact]
    public async Task SignUp_WhenModelStateIsNotValid_ShouldReturnBadRequest()
    {
        // Arrange
        var inputData = new SignUpModel {
            Email = "emailWrong",
            Password = string.Empty,
            UserName = StubData.UserName
        };
        _systemUnderTest.ModelState.AddModelError("Email", ErrorCodes.InvalidEmail);
        _systemUnderTest.ModelState.AddModelError("Password", ErrorCodes.InvalidPassword);

        // Act
        var result = await _systemUnderTest.SignUp(inputData, false) as ObjectResult;

        // Assert
        await _mockAuthService.DidNotReceiveWithAnyArgs().SignUp(default!);
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
        result.Should().NotBeNull();
        result!.Value.Should().BeOfType<BadRequestHttpErrorInfo>();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        (result!.Value as BadRequestHttpErrorInfo)!.ErrorCode.Should().Be(ErrorCodes.ValidationsFailed);
        (result!.Value as BadRequestHttpErrorInfo)!.Errors!.Select(e => e.ErrorCode)
            .Should().BeEquivalentTo([ErrorCodes.InvalidEmail, ErrorCodes.InvalidPassword]);
    }

    [Theory]
    [InlineData(ErrorCodes.UnknownError, 0)]
    [InlineData(ErrorCodes.EmailAlreadyInUse, 0)]
    [InlineData(ErrorCodes.AuthorizarionMissing, 0)]
    [InlineData(ErrorCodes.AuthorizationFailed, 0)]
    [InlineData("Exception Found!", 1)]
    public async Task SignUp_WhenAuthServiceFails_ReturnsError(string errorCode, int loggerCalls)
    {
        // Arrange
        var inputData = new SignUpModel {
            Email = StubData.Email,
            Password = StubData.Password,
            UserName = StubData.UserName
        };

        _mockAuthService.SignUp(inputData).Returns(new AppError(errorCode));

        // Act
        var result = await _systemUnderTest.SignUp(inputData, false) as ObjectResult;

        // Assert
        await _mockAuthService.Received(1).SignUp(inputData);
        result.Should().NotBeNull();
        result!.Value.Should().BeOfType<HttpErrorInfo>();
        var expectedErrorInfo = HttpResponses.TryGetErrorInfo(errorCode);
        (result!.Value as HttpErrorInfo)!.ErrorCode.Should().Be(expectedErrorInfo.ErrorCode);
        result.StatusCode.Should().Be(expectedErrorInfo.HttpStatusCode);

        if (loggerCalls > 0)
            _mockLogger.Received(loggerCalls).LogError(errorCode);
        else
            _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Theory]
    [ClassData(typeof(SignUpModelInputData))]
    public void SignUp_WhenModelStateIsNotValid_ValidationShouldNotPass(string email, string password, string name)
    {
        // Arrange
        var inputData = new SignUpModel {
            Email = email,
            Password = password,
            UserName = name
        };
        var context = new ValidationContext(inputData, null, null);
        var results = new List<ValidationResult>();

        // Act
        var isModelStateValid = Validator.TryValidateObject(inputData, context, results, true);

        // Assert
        isModelStateValid.Should().BeFalse();
        var possibleErrorMessages = new List<string> {
            ErrorCodes.InvalidEmail,
            ErrorCodes.InvalidPassword,
            ErrorCodes.InvalidUserName
        };
        var errorCodesFromValidation = results.Select(e => e.ErrorMessage);
        possibleErrorMessages.Intersect(errorCodesFromValidation).Should().NotBeEmpty();
    }

    [Fact]
    public void SignUp_WhenModelStateIsValid_ValidationShouldPass()
    {
        // Arrange
        var inputData = new SignUpModel {
            Email = StubData.Email,
            Password = StubData.Password,
            UserName = StubData.UserName
        };
        var context = new ValidationContext(inputData, null, null);
        var results = new List<ValidationResult>();

        // Act
        var isModelStateValid = Validator.TryValidateObject(inputData, context, results, true);

        // Assert
        isModelStateValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    internal class SignInModelInputData : TheoryData<string?, string?>
    {
        public SignInModelInputData()
        {
            var testParams = new ITestParam[] {
                new TestParam<string> {
                    Name = "Email",
                    PossibleValues = [StubData.Email, "emailWrong", null!, string.Empty]
                },
                new TestParam<string> {
                    Name = "Password",
                    PossibleValues = [StubData.Password, "wrong", null!, string.Empty,]
                }
            };

            // skip the first and only valid combination
            var combinations = TestCombinationsGenerator.Combine(testParams).Skip(1);
            foreach (var combination in combinations) {
                Add(combination[0] as string, combination[1] as string);
            }
        }
    }

    internal class SignUpModelInputData : TheoryData<string?, string?, string?>
    {
        public SignUpModelInputData()
        {
            var testParams = new ITestParam[] {
                 new TestParam<string> {
                    Name = "Email",
                    PossibleValues = [StubData.Email, "emailWrong", null!, string.Empty]
                },
                new TestParam<string> {
                    Name = "Password",
                    PossibleValues = [StubData.Password, "wrong", null!, string.Empty,]
                },
                new TestParam<string> {
                    Name = "Username",
                    PossibleValues = [StubData.UserName, "pin", null!, string.Empty,]
                }
            };

            // skip the first and only valid combination
            var combinations = TestCombinationsGenerator.Combine(testParams).Skip(1);
            foreach (var combination in combinations) {
                Add(combination[0] as string, combination[1] as string, combination[2] as string);
            }
        }
    }

    private JsonWebToken BuildJwt(
        long userId = StubData.UserId, string email = StubData.Email,
        string token = StubData.JwtToken, string refreshToken = StubData.RefreshToken
    ) {
        return new JsonWebToken {
            UserId = userId,
            Email = email,
            ExpiresIn = 3600,
            Token = token,
            RefreshToken = refreshToken
        };
    }

    private UserTokenIdentity BuildUserTokenIdentity(
        long userId = StubData.UserId, string email = StubData.Email, string name = StubData.UserName
    ) {
        return new UserTokenIdentity {
            UserId = userId,
            Email = email,
            Name = name
        };
    }

    private UserToken BuildUserToken(long userId = StubData.UserId, string token = StubData.RefreshToken)
    {
        return new UserToken {
            Id = 1,
            UserId = userId,
            TokenTypeId = (long) Domain.Enums.TokenType.RefreshToken,
            Token = token,
            ExpiryTime = DateTime.Now.AddYears(1)
        };
    }

    private User BuildUser(
        long userId = StubData.UserId, string email = StubData.Email,
        string name = StubData.UserName, string password = StubData.HashedPassword
    ) {
        return new User {
            Id = userId,
            Email = email,
            Password = password,
            Name = name,
            CreationDate = DateTime.Now,
            ModificationDate = null
        };
    }
}
