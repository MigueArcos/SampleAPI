using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Contracts;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Domain.UnitOfWork;
using ArchitectureTest.Tests.Shared.Mocks;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

namespace ArchitectureTest.Tests.Domain {
    public class UsersDomainTests {
        private readonly MockRepository<User> mockUsersRepo;
        private readonly MockRepository<UserToken> mockUsersTokenRepo;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<IJwtManager> mockJwtManager;
        private readonly Mock<IPasswordHasher> mockPasswordHasher;
        private const string email = "anyEmail@anyDomain.com", password = "anyPassword", name = "anyName";
        private const string randomToken = "eyzhdhhdhd.fhfhhf.fggg", randomRefreshToken = "4nyR3fr35hT0k3n";
        private const long userId = 1;
        public UsersDomainTests() {
            mockUsersRepo = new MockRepository<User>();

            mockUsersTokenRepo = new MockRepository<UserToken>();
            mockUsersTokenRepo.Setup(r => r.Post(It.IsAny<UserToken>())).Returns(Task.FromResult(It.IsAny<UserToken>()));

            mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Repository<User>()).Returns(mockUsersRepo.Object);
            mockUnitOfWork.Setup(uow => uow.Repository<UserToken>()).Returns(mockUsersTokenRepo.Object);

            mockJwtManager = new Mock<IJwtManager>();

            mockPasswordHasher = new Mock<IPasswordHasher>();
        }
        [Fact]
        public async Task UsersDomain_SignIn_ReturnJwt() {
            // Arrange
            var userInfo = new User { Id = userId, Email = email };
            var jwtMockResult = new JsonWebToken { UserId = userId, Email = email, ExpiresIn = 3600, Token = randomToken };
            mockJwtManager.Setup(jwtManager => jwtManager.GenerateToken(It.IsAny<JwtUser>())).Returns(jwtMockResult);
            mockUsersRepo.SetupGetSingleResultInList(userInfo);
            mockPasswordHasher.Setup(pH => pH.Check(It.IsAny<string>(), It.IsAny<string>())).Returns((true, true));
            var requestData = new SignInModel { Email = email, Password = password };
            var usersDomain = new UsersDomain(mockUnitOfWork.Object, mockJwtManager.Object, mockPasswordHasher.Object);

            // Act
            var result = await usersDomain.SignIn(requestData);

            // Assert
            mockUsersRepo.VerifyGetCalls(Times.Once());
            mockPasswordHasher.Verify(pH => pH.Check(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            mockJwtManager.Verify(jM => jM.GenerateToken(It.IsAny<JwtUser>()), Times.Once());
            mockUsersTokenRepo.VerifyPostCalls(Times.Once());
            Assert.Equal(userId, result.UserId);
            Assert.Equal(jwtMockResult.Token, result.Token);
        }
        [Fact]
        public async Task UsersDomain_SignIn_ThrowsUserNotFound() {
            // Arrange
            mockUsersRepo.SetupGetMultipleResults(new List<User>());
            var requestData = new SignInModel { Email = email, Password = password };
            var usersDomain = new UsersDomain(mockUnitOfWork.Object, mockJwtManager.Object, mockPasswordHasher.Object);

            // Act
            Task act() => usersDomain.SignIn(requestData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockUsersRepo.VerifyGetCalls(Times.Once());
            mockPasswordHasher.Verify(pH => pH.Check(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            mockJwtManager.Verify(jM => jM.GenerateToken(It.IsAny<JwtUser>()), Times.Never());
            mockUsersTokenRepo.VerifyPostCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.UserNotFound.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(404, ErrorStatusCode.UserNotFound.HttpStatusCode);
        }
        [Fact]
        public async Task UsersDomain_SignIn_ThrowsWrongPassword() {
            // Arrange
            var userInfo = new User { Id = userId, Email = email };
            mockUsersRepo.SetupGetSingleResultInList(userInfo);
            mockPasswordHasher.Setup(pH => pH.Check(It.IsAny<string>(), It.IsAny<string>())).Returns((false, true));
            var requestData = new SignInModel { Email = email, Password = password };
            var usersDomain = new UsersDomain(mockUnitOfWork.Object, mockJwtManager.Object, mockPasswordHasher.Object);

            // Act
            Task act() => usersDomain.SignIn(requestData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockUsersRepo.VerifyGetCalls(Times.Once());
            mockPasswordHasher.Verify(pH => pH.Check(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            mockJwtManager.Verify(jM => jM.GenerateToken(It.IsAny<JwtUser>()), Times.Never());
            mockUsersTokenRepo.VerifyPostCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.WrongPassword.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(400, ErrorStatusCode.WrongPassword.HttpStatusCode);
        }
        [Fact]
        public async Task UsersDomain_SignUp_ReturnJwt() {
            // Arrange
            var userInfo = new User { Id = userId, Email = email };
            var jwtMockResult = new JsonWebToken { UserId = userId, Email = email, ExpiresIn = 3600, Token = randomToken };
            mockUsersRepo.SetupGetMultipleResults(null);
            mockUsersRepo.Setup(uR => uR.Post(It.IsAny<User>())).Returns(Task.FromResult(userInfo));
            mockJwtManager.Setup(jwtManager => jwtManager.GenerateToken(It.IsAny<JwtUser>())).Returns(jwtMockResult);
            var requestData = new SignUpModel { Email = email, Password = password, UserName = name };
            var usersDomain = new UsersDomain(mockUnitOfWork.Object, mockJwtManager.Object, mockPasswordHasher.Object);

            // Act
            var result = await usersDomain.SignUp(requestData);

            // Assert
            mockUsersRepo.VerifyGetCalls(Times.Once());
            mockUsersRepo.VerifyPostCalls(Times.Once());
            mockJwtManager.Verify(jM => jM.GenerateToken(It.IsAny<JwtUser>()), Times.Once());
            mockUsersTokenRepo.VerifyPostCalls(Times.Once());
            Assert.Equal(userId, result.UserId);
            Assert.Equal(jwtMockResult.Token, result.Token);
        }
        [Fact]
        public async Task UsersDomain_SignUp_ThrowsEmailInUse() {
            // Arrange
            var userInfo = new User { Id = userId, Email = email };
            mockUsersRepo.SetupGetSingleResultInList(userInfo);
            mockUsersRepo.Setup(uR => uR.Post(It.IsAny<User>())).Returns(Task.FromResult(new User()));
            var requestData = new SignUpModel { Email = email, Password = password, UserName = name };
            var usersDomain = new UsersDomain(mockUnitOfWork.Object, mockJwtManager.Object, mockPasswordHasher.Object);

            // Act
            Task act() => usersDomain.SignUp(requestData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockUsersRepo.VerifyGetCalls(Times.Once());
            mockUsersRepo.VerifyPostCalls(Times.Never());
            mockJwtManager.Verify(jM => jM.GenerateToken(It.IsAny<JwtUser>()), Times.Never());
            mockUsersTokenRepo.VerifyPostCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EmailAlreadyInUse.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(400, ErrorStatusCode.EmailAlreadyInUse.HttpStatusCode);
        }
    }
}
