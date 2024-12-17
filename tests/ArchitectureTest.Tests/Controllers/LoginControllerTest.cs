using ArchitectureTest.Domain.Models;
using ArchitectureTest.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using ArchitectureTest.Tests.Shared;
using ArchitectureTest.Web.Configuration;
using Microsoft.Extensions.Logging;
using ArchitectureTest.Domain.Services.Application.AuthService;
using ArchitectureTest.Domain.Errors;

namespace ArchitectureTest.Tests.Controllers;

public class LoginControllerTest {
    private readonly Mock<IAuthService> mockAuthService;
    private readonly LoginController loginController;
    private const string email = "anyEmail@anyDomain.com", password = "anyPassword", name = "anyName";
    private const string randomToken = "eyzhdhhdhd.fhfhhf.fggg", randomRefreshToken = "4nyR3fr35hT0k3n";
    private const long userId = 1;
    public LoginControllerTest() {
        mockAuthService = new Mock<IAuthService>();
        loginController = new LoginController(mockAuthService.Object, new Mock<ILogger<LoginController>>().Object);
    }

    [Fact]
    public async Task LoginController_SignIn_ReturnsJwt() {
        // Arrange
        var jwtMockResult = new JsonWebToken {
            UserId = userId,
            Email = email,
            ExpiresIn = 3600,
            Token = randomToken,
            RefreshToken = randomRefreshToken
        };
        var requestData = new SignInModel { Email = email, Password = password };
        mockAuthService.Setup(uD => uD.SignIn(It.IsAny<SignInModel>())).ReturnsAsync(jwtMockResult);
        // Act
        var result = await loginController.SignIn(requestData, false) as ObjectResult;

        // Assert
        mockAuthService.Verify(uD => uD.SignIn(It.IsAny<SignInModel>()), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<JsonWebToken>(result.Value);
        Assert.Equal(userId, (result.Value as JsonWebToken).UserId);
        Assert.Equal(jwtMockResult.Token, (result.Value as JsonWebToken).Token);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task LoginController_SignIn_ThrowsBadRequestModelStateNotValid() {
        // Arrange
        var requestData = new SignInModel { Email = "emailWrong", Password = password };
        loginController.ModelState.AddModelError("Email", ErrorMessages.InvalidEmail);
        // Act
        var result = await loginController.SignIn(requestData, false) as ObjectResult;

        // Assert
        mockAuthService.Verify(uD => uD.SignIn(It.IsAny<SignInModel>()), Times.Never());
        Assert.NotNull(result);
        Assert.IsType<BadRequestHttpErrorInfo>(result.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LoginController_SignIn_ReturnsUnknownErrorOnUnhandledException(bool useCustomErrorCode) {
        // Arrange
        var requestData = new SignInModel { Email = email, Password = password };
        if (useCustomErrorCode) {
            mockAuthService.Setup(uD => uD.SignIn(It.IsAny<SignInModel>())).ReturnsAsync(new AppError(ErrorCodes.UnknownError));
        }
        else {
            mockAuthService.Setup(uD => uD.SignIn(It.IsAny<SignInModel>())).ReturnsAsync(new AppError("AnyErrorCode"));
        }
        
        // Act
        var result = await loginController.SignIn(requestData, false) as ObjectResult;

        // Assert
        mockAuthService.Verify(uD => uD.SignIn(It.IsAny<SignInModel>()), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<HttpErrorInfo>(result.Value);
        Assert.Equal(ErrorMessages.UnknownError, (result.Value as HttpErrorInfo).Message);
        Assert.Equal(ErrorCodes.UnknownError, (result.Value as HttpErrorInfo).ErrorCode);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetSignInModelTests))]
    public void LoginController_SignIn_ModelStateNotValid(string email, string password) {
        //Arrange
        var requestData = new SignInModel { Email = email, Password = password };
        var context = new ValidationContext(requestData, null, null);
        var results = new List<ValidationResult>();

        //Act
        var isModelStateValid = Validator.TryValidateObject(requestData, context, results, true);

        //Assert
        Assert.False(isModelStateValid);
        var possibleErrorMessages = new List<string> {
            ErrorCodes.InvalidEmail,
            ErrorCodes.InvalidPassword
        };
        Assert.Contains(results[0].ErrorMessage, possibleErrorMessages);
    }

    [Fact]
    public void LoginController_SignIn_ModelStateValid() {
        //Arrange
        var requestData = new SignInModel { Email = email, Password = password };
        var context = new ValidationContext(requestData, null, null);
        var results = new List<ValidationResult>();

        //Act
        var isModelStateValid = Validator.TryValidateObject(requestData, context, results, true);

        //Assert
        Assert.True(isModelStateValid);
        Assert.Empty(results);
    }

    [Fact]
    public async Task LoginController_SignUp_ReturnJwt() {
        // Arrange
        var jwtMockResult = new JsonWebToken {
            UserId = userId,
            Email = email,
            ExpiresIn = 3600,
            Token = randomToken,
            RefreshToken = randomRefreshToken
        };
        var requestData = new SignUpModel { Email = email, Password = password, UserName = name };
        mockAuthService.Setup(uD => uD.SignUp(It.IsAny<SignUpModel>())).ReturnsAsync(jwtMockResult);
        // Act
        var result = await loginController.SignUp(requestData, false) as ObjectResult;

        // Assert
        mockAuthService.Verify(uD => uD.SignUp(It.IsAny<SignUpModel>()), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<JsonWebToken>(result.Value);
        Assert.Equal(userId, (result.Value as JsonWebToken).UserId);
        Assert.Equal(jwtMockResult.Token, (result.Value as JsonWebToken).Token);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task LoginController_SignUp_ThrowsBadRequestModelStateNotValid() {
        // Arrange
        var requestData = new SignUpModel { Email = "emailWrong", Password = password, UserName = name };
        loginController.ModelState.AddModelError("Email", ErrorMessages.InvalidEmail);
        // Act
        var result = await loginController.SignUp(requestData, false) as ObjectResult;

        // Assert
        mockAuthService.Verify(uD => uD.SignUp(It.IsAny<SignUpModel>()), Times.Never());
        Assert.NotNull(result);
        Assert.IsType<BadRequestHttpErrorInfo>(result.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LoginController_SignUp_ReturnsUnknownErrorOnUnhandledException(bool useCustomErrorCode) {
        // Arrange
        var requestData = new SignUpModel { Email = email, Password = password, UserName = name };
        if (useCustomErrorCode) {
            mockAuthService.Setup(uD => uD.SignUp(It.IsAny<SignUpModel>())).ReturnsAsync(new AppError(ErrorCodes.UnknownError));
        }
        else {
            mockAuthService.Setup(uD => uD.SignUp(It.IsAny<SignUpModel>())).ReturnsAsync(new AppError("AnyErrorCode"));
        }

        // Act
        var result = await loginController.SignUp(requestData, false) as ObjectResult;

        // Assert
        mockAuthService.Verify(uD => uD.SignUp(It.IsAny<SignUpModel>()), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<HttpErrorInfo>(result.Value);
        Assert.Equal(ErrorMessages.UnknownError, (result.Value as HttpErrorInfo).Message);
        Assert.Equal(ErrorCodes.UnknownError, (result.Value as HttpErrorInfo).ErrorCode);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetSignUpModelTests))]
    public void LoginController_SignUp_ModelStateNotValid(string email, string password, string name) {
        //Arrange
        var requestData = new SignUpModel { Email = email, Password = password, UserName = name };
        var context = new ValidationContext(requestData, null, null);
        var results = new List<ValidationResult>();

        //Act
        var isModelStateValid = Validator.TryValidateObject(requestData, context, results, true);

        //Assert
        Assert.False(isModelStateValid);
        var possibleErrorMessages = new List<string> {
            ErrorCodes.InvalidEmail,
            ErrorCodes.InvalidPassword,
            ErrorCodes.InvalidUserName
        };
        Assert.Contains(results[0].ErrorMessage, possibleErrorMessages);
    }

    [Fact]
    public void LoginController_SignUp_ModelStateValid() {
        //Arrange
        var requestData = new SignUpModel { Email = email, Password = password, UserName = name };
        var context = new ValidationContext(requestData, null, null);
        var results = new List<ValidationResult>();

        //Act
        var isModelStateValid = Validator.TryValidateObject(requestData, context, results, true);

        //Assert
        Assert.True(isModelStateValid);
        Assert.Empty(results);
    }

    public static IEnumerable<object[]> GetSignInModelTests() {
        var testParams = new TestParam[] {
            new TestParam {
                Name = "Email",
                PossibleValues = new object[] { email, "emailWrong", null, string.Empty }
            },
            new TestParam {
                Name = "Password",
                PossibleValues = new object[] { password, "wrong", null, string.Empty, }
            }
        };
        return TestCombinationsGenerator.Combine(testParams).Skip(1);
    }

    public static IEnumerable<object[]> GetSignUpModelTests() {
        var testParams = new TestParam[] {
            new TestParam {
                Name = "Email",
                PossibleValues = new object[] { email, "emailWrong", null, string.Empty }
            },
            new TestParam {
                Name = "Password",
                PossibleValues = new object[] { password, "wrong", null, string.Empty, }
            },
            new TestParam {
                Name = "Username",
                PossibleValues = new object[] { name, "pin", null, string.Empty, }
            }
        };
        return TestCombinationsGenerator.Combine(testParams).Skip(1);
    }
}
