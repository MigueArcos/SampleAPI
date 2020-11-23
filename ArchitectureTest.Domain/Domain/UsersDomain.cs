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
		private readonly IJwtManager jwtManager;
		private readonly IPasswordHasher passwordHasher;
		public UsersDomain(IUnitOfWork unitOfWork, IJwtManager jwtManager, IPasswordHasher passwordHasher) {
			this.jwtManager = jwtManager;
			this.passwordHasher = passwordHasher;
			usersRepository = unitOfWork.Repository<User>();
		}

		public async Task<JsonWebToken> SignIn(SignInModel signInModel){
			try{
				var userSearch = await usersRepository.Get(u => u.Email == signInModel.Email);
				if (userSearch == null || userSearch.Count == 0) throw ErrorStatusCode.UserNotFound;
				User user = userSearch[0];
				var (Verified, NeedsUpgrade) = passwordHasher.Check(user.Password, signInModel.Password);
				if (!Verified) throw ErrorStatusCode.WrongPassword;
				return await jwtManager.GenerateToken(new JwtUser {
					Name = user.Name,
					Email = user.Email,
					Id = user.Id
				});
			}
			catch (Exception exception) {
				throw Utils.HandleException(exception);
			}
		}

		public async Task<JsonWebToken> SignUp(SignUpModel signUpModel) {
			try{
				var userSearch = await usersRepository.Get(u => u.Email == signUpModel.Email);
				if (userSearch != null && userSearch.Count > 0) throw ErrorStatusCode.EmailAlreadyInUse;
				var user = await usersRepository.Post(new User {
					Name = signUpModel.UserName,
					Email = signUpModel.Email,
					Password = passwordHasher.Hash(signUpModel.Password)
				});
				return await jwtManager.GenerateToken(new JwtUser {
					Name = user.Name,
					Email = user.Email,
					Id = user.Id
				});
			}
			catch (Exception exception) {
				throw Utils.HandleException(exception);
			}
		}
	}
}
