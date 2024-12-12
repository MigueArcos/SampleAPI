using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using System;
using System.Threading.Tasks;
using ArchitectureTest.Domain.ServiceLayer.JwtManager;
using ArchitectureTest.Domain.ServiceLayer.PasswordHasher;
using ArchitectureTest.Domain.Models.Enums;

namespace ArchitectureTest.Domain.ServiceLayer.AuthService;

public class AuthService : IAuthService {
	private readonly IRepository<long, User> _usersRepository;
	private readonly IRepository<long, UserToken> _tokensRepository;
	private readonly IJwtManager _jwtManager;
	private readonly IPasswordHasher _passwordHasher;

	public AuthService(IUnitOfWork unitOfWork, IJwtManager jwtManager, IPasswordHasher passwordHasher) {
		_jwtManager = jwtManager;
		_passwordHasher = passwordHasher;
		_usersRepository = unitOfWork.Repository<User>();
		_tokensRepository = unitOfWork.Repository<UserToken>();
	}

	public async Task<JsonWebToken> SignIn(SignInModel signInModel) {
		var user = await _usersRepository.FindSingle(u => u.Email == signInModel.Email) 
			?? throw new Exception(ErrorCodes.UserNotFound);
	
		var (Verified, NeedsUpgrade) = _passwordHasher.Check(user.Password, signInModel.Password);
		if (!Verified) throw new Exception(ErrorCodes.WrongPassword);
		var userJwt = await CreateUserJwt(user.Id, user.Email, user.Name);
		return userJwt;
	}

	public async Task<JsonWebToken> SignUp(SignUpModel signUpModel) {
		var userFound = await _usersRepository.FindSingle(u => u.Email == signUpModel.Email);
		if (userFound != null)
			throw new Exception(ErrorCodes.EmailAlreadyInUse);

		var newUser = await _usersRepository.Add(new User {
			Name = signUpModel.UserName,
			Email = signUpModel.Email,
			Password = _passwordHasher.Hash(signUpModel.Password)
		});
		var userJwt = await CreateUserJwt(newUser.Id, newUser.Email, newUser.Name);
		return userJwt;
	}

	private async Task<JsonWebToken> CreateUserJwt(long userId, string email, string name) {
		var userJwt = _jwtManager.GenerateToken(new JwtUser {
			Name = name,
			Email = email,
			Id = userId
		});
		await _tokensRepository.Add(new UserToken {
			UserId = userJwt.UserId,
			Token = userJwt.RefreshToken,
			TokenTypeId = (long)Data.Enums.TokenType.RefreshToken,
			ExpiryTime = DateTime.Now.AddSeconds(_jwtManager.RefreshTokenTTLSeconds)
		});
		return userJwt;
	}
}
