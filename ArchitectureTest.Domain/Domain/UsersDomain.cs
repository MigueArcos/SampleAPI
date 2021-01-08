using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Contracts;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Repositories.BasicRepo;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Domain.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Domain {
	public class UsersDomain {
		private readonly IRepository<User> usersRepository;
        private readonly IRepository<UserToken> tokensRepository;
        private readonly IJwtManager jwtManager;
		private readonly IPasswordHasher passwordHasher;
		public UsersDomain(IUnitOfWork unitOfWork, IJwtManager jwtManager, IPasswordHasher passwordHasher) {
			this.jwtManager = jwtManager;
			this.passwordHasher = passwordHasher;
            this.usersRepository = unitOfWork.Repository<User>();
			this.tokensRepository = unitOfWork.Repository<UserToken>();
		}

		public async Task<JsonWebToken> SignIn(SignInModel signInModel) {
			var userSearch = await usersRepository.Get(u => u.Email == signInModel.Email);
			if (userSearch == null || userSearch.Count == 0) throw ErrorStatusCode.UserNotFound;
			User user = userSearch[0];
			var (Verified, NeedsUpgrade) = passwordHasher.Check(user.Password, signInModel.Password);
			if (!Verified) throw ErrorStatusCode.WrongPassword;
            var userJwt = await CreateUserJwt(user.Id, user.Email, user.Name);
            return userJwt;
		}

		public async Task<JsonWebToken> SignUp(SignUpModel signUpModel) {
			var userSearch = await usersRepository.Get(u => u.Email == signUpModel.Email);
			if (userSearch != null && userSearch.Count > 0) throw ErrorStatusCode.EmailAlreadyInUse;
			var user = await usersRepository.Post(new User {
				Name = signUpModel.UserName,
				Email = signUpModel.Email,
				Password = passwordHasher.Hash(signUpModel.Password)
			});
            var userJwt = await CreateUserJwt(user.Id, user.Email, user.Name);
            return userJwt;
        }
        private async Task<JsonWebToken> CreateUserJwt(long userId, string email, string name) {
            var userJwt = jwtManager.GenerateToken(new JwtUser {
                Name = name,
                Email = email,
                Id = userId
            });
            await tokensRepository.Post(new UserToken {
                UserId = userJwt.UserId,
                Token = userJwt.RefreshToken,
                TokenTypeId = (long)Data.Enums.TokenType.RefreshToken,
                ExpiryTime = DateTime.Now.AddSeconds(jwtManager.RefreshTokenTTLSeconds)
            });
            return userJwt;
        }
	}
}
