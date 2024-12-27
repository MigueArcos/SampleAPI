using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models.Application;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Domain.Services.Application.AuthService;
using ArchitectureTest.Domain.Services.Infrastructure;
using ArchitectureTest.TestUtils;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace ArchitectureTest.Domain.Tests.ApplicationServices;

public class AuthServiceTests {
    private readonly IRepository<User> _mockUsersRepo;
    private readonly IRepository<UserToken> _mockUsersTokenRepo;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IJwtManager _mockJwtManager;
    private readonly IPasswordHasher _mockPasswordHasher;
    private readonly ILogger<AuthService> _mockLogger;

    private readonly AuthService _systemUnderTest;

    public AuthServiceTests() {
        var jwtConfig = new Dictionary<string, string?>{
            {"ConfigData:Jwt:TokenTTLSeconds", "3600"},
            {"ConfigData:Jwt:RefreshTokenTTLHours", "720"},
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(jwtConfig)
            .Build();
        
        _mockUsersRepo = Substitute.For<IRepository<User>>();
        _mockUsersTokenRepo = Substitute.For<IRepository<UserToken>>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockJwtManager = Substitute.For<IJwtManager>();
        _mockPasswordHasher = Substitute.For<IPasswordHasher>();
        _mockLogger = Substitute.For<ILogger<AuthService>>();

        _mockUnitOfWork
            .Repository<User>()
            .Returns(_mockUsersRepo);
        _mockUnitOfWork
            .Repository<UserToken>()
            .Returns(_mockUsersTokenRepo);

        _systemUnderTest = new AuthService(_mockUnitOfWork, _mockJwtManager, _mockPasswordHasher, configuration, _mockLogger);
    }

    [Fact]
    public async Task SignIn_WhenEverythingIsOK_ReturnsJwt()
    {
        // Arrange
        var inputData = new SignInModel {
            Email = StubData.Email,
            Password = StubData.Password
        };

        var userInfo = BuildUser();
        var tokenIdentity = BuildUserTokenIdentity();
        var jwtToken = BuildJwt();
        var userToken = BuildUserToken();
        
        // Using Func<T> instead of Expression<Func<T>> to be able to use optional arguments and collectionExpressions
        Func<UserTokenIdentity, bool> generateTokenInputValidator = arg => StubData.JsonCompare(arg, tokenIdentity);
        Func<UserToken, bool> repoAddUserTokenInputValidator = arg => 
            StubData.JsonCompare(arg, userToken, [nameof(userToken.Id), nameof(userToken.ExpiryTime)]);

        _mockUsersRepo.FindSingle(Arg.Any<Expression<Func<User, bool>>>()).Returns(userInfo);
        _mockPasswordHasher.Check(userInfo.Password, inputData.Password).Returns((true, true));
        _mockJwtManager.GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a))).Returns(jwtToken);
        _mockUsersTokenRepo.Create(Arg.Is<UserToken>(a => repoAddUserTokenInputValidator(a))).Returns(userToken);
        
        // Act
        var result = await _systemUnderTest.SignIn(inputData);

        // Assert
        await _mockUsersRepo.Received(1).FindSingle(Arg.Any<Expression<Func<User, bool>>>());
        _mockPasswordHasher.Received(1).Check(userInfo.Password, inputData.Password);
        _mockJwtManager.Received(1).GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a)));
        await _mockUsersTokenRepo.Received(1).Create(Arg.Is<UserToken>(a => repoAddUserTokenInputValidator(a)));

        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(jwtToken.UserId);
        result.Value!.Token.Should().Be(jwtToken.Token);
    }

    [Fact]
    public async Task SignIn_WhenJwtManagerFailsToGenerateToken_ReturnsError()
    {
        // Arrange
        var inputData = new SignInModel {
            Email = StubData.Email,
            Password = StubData.Password
        };

        var userInfo = BuildUser();
        var tokenIdentity = BuildUserTokenIdentity();

        Func<UserTokenIdentity, bool> generateTokenInputValidator = arg => StubData.JsonCompare(arg, tokenIdentity);

        _mockUsersRepo.FindSingle(Arg.Any<Expression<Func<User, bool>>>()).Returns(userInfo);
        _mockPasswordHasher.Check(userInfo.Password, inputData.Password).Returns((true, true));
        _mockJwtManager.GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a)))
            .Returns(new AppError(ErrorCodes.CannotGenerateJwtToken));

        // Act
        var result = await _systemUnderTest.SignIn(inputData);

        // Assert
        await _mockUsersRepo.Received(1).FindSingle(Arg.Any<Expression<Func<User, bool>>>());
        _mockPasswordHasher.Received(1).Check(userInfo.Password, inputData.Password);
        _mockJwtManager.Received(1).GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a)));
        await _mockUsersTokenRepo.DidNotReceiveWithAnyArgs().Create(default!);

        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.CannotGenerateJwtToken);
    }

    [Fact]
    public async Task SignIn_WhenPasswordIsWrong_ReturnsError()
    {
        // Arrange
        var inputData = new SignInModel {
            Email = StubData.Email,
            Password = StubData.Password
        };

        var userInfo = BuildUser();

        _mockUsersRepo.FindSingle(Arg.Any<Expression<Func<User, bool>>>()).Returns(userInfo);
        _mockPasswordHasher.Check(userInfo.Password, inputData.Password).Returns((false, true));

        // Act
        var result = await _systemUnderTest.SignIn(inputData);

        // Assert
        await _mockUsersRepo.Received(1).FindSingle(Arg.Any<Expression<Func<User, bool>>>());
        _mockPasswordHasher.Received(1).Check(userInfo.Password, inputData.Password);
        _mockJwtManager.DidNotReceiveWithAnyArgs().GenerateToken(default!);
        await _mockUsersTokenRepo.DidNotReceiveWithAnyArgs().Create(default!);

        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.WrongPassword);
    }

    [Fact]
    public async Task SignIn_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        var inputData = new SignInModel {
            Email = StubData.Email,
            Password = StubData.Password
        };
        _mockUsersRepo.FindSingle(Arg.Any<Expression<Func<User, bool>>>()).Returns((User) default!);
        
        // Act
        var result = await _systemUnderTest.SignIn(inputData);

        // Assert
        await _mockUsersRepo.Received(1).FindSingle(Arg.Any<Expression<Func<User, bool>>>());
        _mockPasswordHasher.DidNotReceiveWithAnyArgs().Check(default!, default!);
        _mockJwtManager.DidNotReceiveWithAnyArgs().GenerateToken(default!);
        await _mockUsersTokenRepo.DidNotReceiveWithAnyArgs().Create(default!);

        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.UserNotFound);
    }

    [Fact]
    public async Task SignUp_WhenEverythingIsOK_ReturnsJwt()
    {
        // Arrange
        var inputData = new SignUpModel {
            Email = StubData.Email,
            UserName = StubData.UserName,
            Password = StubData.Password
        };
        
        var userInfo = BuildUser();
        var tokenIdentity = BuildUserTokenIdentity();
        var jwtToken = BuildJwt();
        var userToken = BuildUserToken();
        
        // Using Func<T> instead of Expression<Func<T>> to be able to use optional arguments and collectionExpressions
        Func<UserTokenIdentity, bool> generateTokenInputValidator = arg => StubData.JsonCompare(arg, tokenIdentity);
        Func<UserToken, bool> repoAddUserTokenInputValidator = arg => 
            StubData.JsonCompare(arg, userToken, [nameof(userToken.Id), nameof(userToken.ExpiryTime)]);
        Func<User, bool> repoAddUserInputValidator = arg =>
            StubData.JsonCompare(arg, userInfo, [nameof(userInfo.Id), nameof(userInfo.CreationDate)]);

        _mockUsersRepo.FindSingle(Arg.Any<Expression<Func<User, bool>>>()).Returns((User) default!);
        _mockPasswordHasher.Hash(inputData.Password).Returns(StubData.HashedPassword);
        _mockUsersRepo.Create(Arg.Is<User>(a => repoAddUserInputValidator(a))).Returns(userInfo);
        _mockJwtManager.GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a))).Returns(jwtToken);
        _mockUsersTokenRepo.Create(Arg.Is<UserToken>(a => repoAddUserTokenInputValidator(a))).Returns(userToken);
         
        // Act
        var result = await _systemUnderTest.SignUp(inputData);

        // Assert
        await _mockUsersRepo.Received(1).FindSingle(Arg.Any<Expression<Func<User, bool>>>());
        _mockPasswordHasher.Received(1).Hash(inputData.Password);
        await _mockUsersRepo.Received(1).Create(Arg.Is<User>(a => repoAddUserInputValidator(a)));
        _mockJwtManager.Received(1).GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a)));
        await _mockUsersTokenRepo.Received(1).Create(Arg.Is<UserToken>(a => repoAddUserTokenInputValidator(a)));

        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(jwtToken.UserId);
        result.Value.Token.Should().Be(jwtToken.Token);
    }

    [Fact]
    public async Task SignUp_WhenJwtManagerFailsToGenerateToken_ReturnsError()
    {
        // Arrange
        var inputData = new SignUpModel {
            Email = StubData.Email,
            UserName = StubData.UserName,
            Password = StubData.Password
        };
        
        var userInfo = BuildUser();
        var tokenIdentity = BuildUserTokenIdentity();
        var jwtToken = BuildJwt();
        var userToken = BuildUserToken();
        
        // Using Func<T> instead of Expression<Func<T>> to be able to use optional arguments and collectionExpressions
        Func<UserTokenIdentity, bool> generateTokenInputValidator = arg => StubData.JsonCompare(arg, tokenIdentity);
        Func<User, bool> repoAddUserInputValidator = arg =>
            StubData.JsonCompare(arg, userInfo, [nameof(userInfo.Id), nameof(userInfo.CreationDate)]);

        _mockUsersRepo.FindSingle(Arg.Any<Expression<Func<User, bool>>>()).Returns((User) default!);
        _mockPasswordHasher.Hash(inputData.Password).Returns(StubData.HashedPassword);
        _mockUsersRepo.Create(Arg.Any<User>()).Returns(userInfo);
        _mockJwtManager.GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a)))
            .Returns(new AppError(ErrorCodes.CannotGenerateJwtToken));
         
        // Act
        var result = await _systemUnderTest.SignUp(inputData);

        // Assert
        await _mockUsersRepo.Received(1).FindSingle(Arg.Any<Expression<Func<User, bool>>>());
        _mockPasswordHasher.Received(1).Hash(inputData.Password);
        await _mockUsersRepo.Received(1).Create(Arg.Is<User>(a => repoAddUserInputValidator(a)));
        _mockJwtManager.Received(1).GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a)));
        await _mockUsersTokenRepo.DidNotReceiveWithAnyArgs().Create(default!);

        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.CannotGenerateJwtToken);
    }

    [Fact]
    public async Task SignUp_WhenEmailIsAlreadyInUse_ReturnsError()
    {
        // Arrange
        var inputData = new SignUpModel {
            Email = StubData.Email,
            UserName = StubData.UserName,
            Password = StubData.Password
        };
        
        var userInfo = BuildUser();
    
        _mockUsersRepo.FindSingle(Arg.Any<Expression<Func<User, bool>>>()).Returns(userInfo);

         
        // Act
        var result = await _systemUnderTest.SignUp(inputData);

        // Assert
        await _mockUsersRepo.Received(1).FindSingle(Arg.Any<Expression<Func<User, bool>>>());
        _mockPasswordHasher.DidNotReceiveWithAnyArgs().Hash(default!);
        await _mockUsersRepo.DidNotReceiveWithAnyArgs().Create(default!);
        _mockJwtManager.DidNotReceiveWithAnyArgs().GenerateToken(default!);
        await _mockUsersTokenRepo.DidNotReceiveWithAnyArgs().Create(default!);

        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.EmailAlreadyInUse);
    }

    [Fact]
    public async Task ExchangeOldTokensForNewToken_WhenEverythingIsOK_ReturnsJwt()
    {
        // Arrange
        var userInfo = BuildUser();
        var tokenIdentity = BuildUserTokenIdentity();
        var jwtToken = BuildJwt();
        var userToken = BuildUserToken();
        
        // Using Func<T> instead of Expression<Func<T>> to be able to use optional arguments and collectionExpressions
        Func<UserTokenIdentity, bool> generateTokenInputValidator = arg => StubData.JsonCompare(arg, tokenIdentity);
        Func<UserToken, bool> repoAddUserTokenInputValidator = arg => 
            StubData.JsonCompare(arg, userToken, [nameof(userToken.Id), nameof(userToken.ExpiryTime)]);

        _mockUsersTokenRepo.FindSingle(Arg.Any<Expression<Func<UserToken, bool>>>()).Returns(userToken);
        _mockJwtManager.ReadToken(StubData.JwtToken, false).Returns((tokenIdentity, null!));
        _mockUsersTokenRepo.DeleteById(userToken.Id).Returns(true);
        _mockJwtManager.GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a))).Returns(jwtToken);
        _mockUsersTokenRepo.Create(Arg.Is<UserToken>(a => repoAddUserTokenInputValidator(a))).Returns(userToken);        

        // Act
        var result = await _systemUnderTest.ExchangeOldTokensForNewToken(StubData.JwtToken, StubData.RefreshToken);

        // Assert
        await _mockUsersTokenRepo.Received(1).FindSingle(Arg.Any<Expression<Func<UserToken, bool>>>());
        _mockJwtManager.Received(1).ReadToken(StubData.JwtToken, false);
        _mockUnitOfWork.Received(1).StartTransaction();
        await _mockUsersTokenRepo.Received(1).DeleteById(userToken.Id);
        _mockJwtManager.Received(1).GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a)));
        await _mockUsersTokenRepo.Received(1).Create(Arg.Is<UserToken>(a => repoAddUserTokenInputValidator(a)));
        _mockUnitOfWork.Received(1).Commit();
        _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);

        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().Be(jwtToken);
    }

    [Fact]
    public async Task ExchangeOldTokensForNewToken_WhenJwtManagerFailsToGenerateToken_ReturnsError()
    {
        // Arrange
        var userInfo = BuildUser();
        var tokenIdentity = BuildUserTokenIdentity();
        var jwtToken = BuildJwt();
        var userToken = BuildUserToken();
        
        Func<UserTokenIdentity, bool> generateTokenInputValidator = arg => StubData.JsonCompare(arg, tokenIdentity);

        _mockUsersTokenRepo.FindSingle(Arg.Any<Expression<Func<UserToken, bool>>>()).Returns(userToken);
        _mockJwtManager.ReadToken(StubData.JwtToken, false).Returns((tokenIdentity, null!));
        _mockUsersTokenRepo.DeleteById(userToken.Id).Returns(true);
        _mockJwtManager.GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a))).Returns(new AppError(ErrorCodes.CannotGenerateJwtToken));

        // Act
        var result = await _systemUnderTest.ExchangeOldTokensForNewToken(StubData.JwtToken, StubData.RefreshToken);

        // Assert
        await _mockUsersTokenRepo.Received(1).FindSingle(Arg.Any<Expression<Func<UserToken, bool>>>());
        _mockJwtManager.Received(1).ReadToken(StubData.JwtToken, false);
        _mockUnitOfWork.Received(1).StartTransaction();
        await _mockUsersTokenRepo.Received(1).DeleteById(userToken.Id);
        _mockJwtManager.Received(1).GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a)));
        await _mockUsersTokenRepo.DidNotReceiveWithAnyArgs().Create(default!);
        _mockUnitOfWork.DidNotReceive().Commit();
        _mockUnitOfWork.Received(1).Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);

        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        // result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.CannotGenerateJwtToken);
    }


    [Fact]
    public async Task ExchangeOldTokensForNewToken_WhenAnExceptionIsThrownDuringTransaction_ReturnsError()
    {
        // Arrange
        var userInfo = BuildUser();
        var tokenIdentity = BuildUserTokenIdentity();
        var jwtToken = BuildJwt();
        var userToken = BuildUserToken();
        var thrownException = new Exception(ErrorCodes.UnknownError);
        
        // Using Func<T> instead of Expression<Func<T>> to be able to use optional arguments and collectionExpressions
        Func<UserTokenIdentity, bool> generateTokenInputValidator = arg => StubData.JsonCompare(arg, tokenIdentity);
        Func<UserToken, bool> repoAddUserTokenInputValidator = arg => 
            StubData.JsonCompare(arg, userToken, [nameof(userToken.Id), nameof(userToken.ExpiryTime)]);

        _mockUsersTokenRepo.FindSingle(Arg.Any<Expression<Func<UserToken, bool>>>()).Returns(userToken);
        _mockJwtManager.ReadToken(StubData.JwtToken, false).Returns((tokenIdentity, null!));
        _mockUsersTokenRepo.DeleteById(userToken.Id).Returns(true);
        _mockJwtManager.GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a))).Returns(jwtToken);
        _mockUsersTokenRepo.Create(Arg.Is<UserToken>(a => repoAddUserTokenInputValidator(a))).Returns(userToken);
        _mockUnitOfWork.When(m => m.Commit()).Throw(thrownException);

        // Act
        var result = await _systemUnderTest.ExchangeOldTokensForNewToken(StubData.JwtToken, StubData.RefreshToken);

        // Assert
        await _mockUsersTokenRepo.Received(1).FindSingle(Arg.Any<Expression<Func<UserToken, bool>>>());
        _mockJwtManager.Received(1).ReadToken(StubData.JwtToken, false);
        _mockUnitOfWork.Received(1).StartTransaction();
        await _mockUsersTokenRepo.Received(1).DeleteById(userToken.Id);
        _mockJwtManager.Received(1).GenerateToken(Arg.Is<UserTokenIdentity>(a => generateTokenInputValidator(a)));
        await _mockUsersTokenRepo.Received(1).Create(Arg.Is<UserToken>(a => repoAddUserTokenInputValidator(a)));
        _mockUnitOfWork.Received(1).Commit();
        _mockLogger.Received(1).LogError(thrownException.Message);
        _mockUnitOfWork.Received(1).Rollback();


        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        // result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.RepoProblem);
    }

    [Fact]
    public async Task ExchangeOldTokensForNewToken_WhenJwtManagerFailsToReadToken_ReturnsError()
    {
        // Arrange
        var userToken = BuildUserToken();
        
        _mockUsersTokenRepo.FindSingle(Arg.Any<Expression<Func<UserToken, bool>>>()).Returns(userToken);
        _mockJwtManager.ReadToken(StubData.JwtToken, false).Returns(new AppError(ErrorCodes.IncompleteJwtTokenData));

        // Act
        var result = await _systemUnderTest.ExchangeOldTokensForNewToken(StubData.JwtToken, StubData.RefreshToken);

        // Assert
        await _mockUsersTokenRepo.Received(1).FindSingle(Arg.Any<Expression<Func<UserToken, bool>>>());
        _mockJwtManager.Received(1).ReadToken(StubData.JwtToken, false);
        _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockUsersTokenRepo.DidNotReceiveWithAnyArgs().DeleteById(default);
        _mockJwtManager.DidNotReceiveWithAnyArgs().GenerateToken(default!);
        await _mockUsersTokenRepo.DidNotReceiveWithAnyArgs().Create(default!);
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
        _mockUnitOfWork.DidNotReceive().Commit();

        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        // result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.IncompleteJwtTokenData);
    }

    [Fact]
    public async Task ExchangeOldTokensForNewToken_WhenRefreshTokenNotFound_ReturnsError()
    {
        // Arrange       
        _mockUsersTokenRepo.FindSingle(Arg.Any<Expression<Func<UserToken, bool>>>()).Returns((UserToken) default!);

        // Act
        var result = await _systemUnderTest.ExchangeOldTokensForNewToken(StubData.JwtToken, StubData.RefreshToken);

        // Assert
        await _mockUsersTokenRepo.Received(1).FindSingle(Arg.Any<Expression<Func<UserToken, bool>>>());
        _mockJwtManager.DidNotReceiveWithAnyArgs().ReadToken(default!, default);
        _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockUsersTokenRepo.DidNotReceiveWithAnyArgs().DeleteById(default);
        _mockJwtManager.DidNotReceiveWithAnyArgs().GenerateToken(default!);
        await _mockUsersTokenRepo.DidNotReceiveWithAnyArgs().Create(default!);
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
        _mockUnitOfWork.DidNotReceive().Commit();

        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        // result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.RefreshTokenExpired);
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
            TokenTypeId = (long) Enums.TokenType.RefreshToken,
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
