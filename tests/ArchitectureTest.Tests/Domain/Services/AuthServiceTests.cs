using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Domain.Services.Application.AuthService;
using ArchitectureTest.Domain.Services.Infrastructure;
using ArchitectureTest.Tests.Shared.Mocks;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchitectureTest.Tests.Domain.Services;

public class AuthServiceTests {
    private readonly MockRepository<User> mockUsersRepo;
    private readonly MockRepository<UserToken> mockUsersTokenRepo;
    private readonly Mock<IUnitOfWork> mockUnitOfWork;
    private readonly Mock<IJwtManager> mockJwtManager;
    private readonly Mock<IPasswordHasher> mockPasswordHasher;
    private const string email = "anyEmail@anyDomain.com", password = "anyPassword", name = "anyName";
    private const string randomToken = "eyzhdhhdhd.fhfhhf.fggg", randomRefreshToken = "4nyR3fr35hT0k3n";
    private const long userId = 1;

    private readonly AuthService _systemUnderTest;

    public AuthServiceTests() {
        var jwtConfig = new Dictionary<string, string>{
            {"ConfigData:Jwt:TokenTTLSeconds", "3600"},
            {"ConfigData:Jwt:RefreshTokenTTLHours", "720"},
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(jwtConfig)
            .Build();
        mockUsersRepo = new MockRepository<User>();

        mockUsersTokenRepo = new MockRepository<UserToken>();
        mockUsersTokenRepo.Setup(r => r.Add(It.IsAny<UserToken>())).Returns(Task.FromResult(It.IsAny<UserToken>()));

        mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Repository<User>()).Returns(mockUsersRepo.Object);
        mockUnitOfWork.Setup(uow => uow.Repository<UserToken>()).Returns(mockUsersTokenRepo.Object);

        mockJwtManager = new Mock<IJwtManager>();

        mockPasswordHasher = new Mock<IPasswordHasher>();

        _systemUnderTest = new AuthService(
            mockUnitOfWork.Object, mockJwtManager.Object, mockPasswordHasher.Object, configuration
        );
    }

    [Fact]
    public async Task AuthService_SignIn_ReturnJwt() {
        // Arrange
        var userInfo = new User { Id = userId, Email = email };
        var jwtMockResult = new JsonWebToken {
            UserId = userId,
            Email = email,
            ExpiresIn = 3600,
            Token = randomToken,
            RefreshToken = randomRefreshToken
        };
        mockJwtManager.Setup(jwtManager => jwtManager.GenerateToken(It.IsAny<JwtUser>())).Returns(jwtMockResult);
        mockUsersRepo.SetupFindSingleResult(userInfo);
        mockPasswordHasher.Setup(pH => pH.Check(It.IsAny<string>(), It.IsAny<string>())).Returns((true, true));
        var requestData = new SignInModel { Email = email, Password = password };

        // Act
        var result = await _systemUnderTest.SignIn(requestData);

        // Assert
        mockUsersRepo.VerifyFindSingleCalls(Times.Once());
        mockPasswordHasher.Verify(pH => pH.Check(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        mockJwtManager.Verify(jM => jM.GenerateToken(It.IsAny<JwtUser>()), Times.Once());
        mockUsersTokenRepo.VerifyAddEntityCalls(Times.Once());
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(jwtMockResult.Token, result.Value.Token);
    }

    [Fact]
    public async Task AuthService_SignIn_ReturnsUserNotFoundError() {
        // Arrange
        mockUsersRepo.SetupFindMultipleResults(new List<User>());
        var requestData = new SignInModel { Email = email, Password = password };

        // Act
        var result = await _systemUnderTest.SignIn(requestData);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockUsersRepo.VerifyFindSingleCalls(Times.Once());
        mockPasswordHasher.Verify(pH => pH.Check(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        mockJwtManager.Verify(jM => jM.GenerateToken(It.IsAny<JwtUser>()), Times.Never());
        mockUsersTokenRepo.VerifyAddEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.UserNotFound, result.Error.Code);
    }

    [Fact]
    public async Task AuthService_SignIn_ReturnsWrongPasswordError() {
        // Arrange
        var userInfo = new User { Id = userId, Email = email };
        mockUsersRepo.SetupFindSingleResult(userInfo);
        mockPasswordHasher.Setup(pH => pH.Check(It.IsAny<string>(), It.IsAny<string>())).Returns((false, true));
        var requestData = new SignInModel { Email = email, Password = password };

        // Act
        var result = await _systemUnderTest.SignIn(requestData);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockUsersRepo.VerifyFindSingleCalls(Times.Once());
        mockPasswordHasher.Verify(pH => pH.Check(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        mockJwtManager.Verify(jM => jM.GenerateToken(It.IsAny<JwtUser>()), Times.Never());
        mockUsersTokenRepo.VerifyAddEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.WrongPassword, result.Error.Code);
    }

    [Fact]
    public async Task AuthService_SignUp_ReturnJwt() {
        // Arrange
        var userInfo = new User { Id = userId, Email = email };
        var jwtMockResult = new JsonWebToken {
            UserId = userId,
            Email = email,
            ExpiresIn = 3600,
            Token = randomToken,
            RefreshToken = randomRefreshToken
        };
        mockUsersRepo.SetupFindSingleResult(null);
        mockUsersRepo.Setup(uR => uR.Add(It.IsAny<User>())).Returns(Task.FromResult(userInfo));
        mockJwtManager.Setup(jwtManager => jwtManager.GenerateToken(It.IsAny<JwtUser>())).Returns(jwtMockResult);
        var requestData = new SignUpModel { Email = email, Password = password, UserName = name };

        // Act
        var result = await _systemUnderTest.SignUp(requestData);

        // Assert
        mockUsersRepo.VerifyFindSingleCalls(Times.Once());
        mockUsersRepo.VerifyAddEntityCalls(Times.Once());
        mockJwtManager.Verify(jM => jM.GenerateToken(It.IsAny<JwtUser>()), Times.Once());
        mockUsersTokenRepo.VerifyAddEntityCalls(Times.Once());
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(jwtMockResult.Token, result.Value.Token);
    }

    [Fact]
    public async Task AuthService_SignUp_ReturnsEmailInUseError() {
        // Arrange
        var userInfo = new User { Id = userId, Email = email };
        mockUsersRepo.SetupFindSingleResult(userInfo);
        mockUsersRepo.Setup(uR => uR.Add(It.IsAny<User>())).Returns(Task.FromResult(new User()));
        var requestData = new SignUpModel { Email = email, Password = password, UserName = name };

        // Act
        var result = await _systemUnderTest.SignUp(requestData);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockUsersRepo.VerifyFindSingleCalls(Times.Once());
        mockUsersRepo.VerifyAddEntityCalls(Times.Never());
        mockJwtManager.Verify(jM => jM.GenerateToken(It.IsAny<JwtUser>()), Times.Never());
        mockUsersTokenRepo.VerifyAddEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EmailAlreadyInUse, result.Error.Code);
    }
}
