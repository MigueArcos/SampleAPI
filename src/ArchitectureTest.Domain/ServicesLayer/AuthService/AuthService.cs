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

	public async Task<Result<JsonWebToken, AppError>> SignIn(SignInModel signInModel) {
		var user = await _usersRepository.FindSingle(u => u.Email == signInModel.Email).ConfigureAwait(false);
		if (user == null)
			return new AppError(ErrorCodes.UserNotFound);

		var (Verified, NeedsUpgrade) = _passwordHasher.Check(user.Password, signInModel.Password);

		if (!Verified)
			return new AppError(ErrorCodes.WrongPassword);

		var userJwt = await CreateUserJwt(user.Id, user.Email, user.Name).ConfigureAwait(false);
		return userJwt;
	}

	public async Task<Result<JsonWebToken, AppError>> SignUp(SignUpModel signUpModel) {
		var userFound = await _usersRepository.FindSingle(u => u.Email == signUpModel.Email).ConfigureAwait(false);
		if (userFound != null)
			return new AppError(ErrorCodes.EmailAlreadyInUse);

		var newUser = await _usersRepository.Add(new User {
			Name = signUpModel.UserName,
			Email = signUpModel.Email,
			Password = _passwordHasher.Hash(signUpModel.Password)
		}).ConfigureAwait(false);

		var userJwt = await CreateUserJwt(newUser.Id, newUser.Email, newUser.Name).ConfigureAwait(false);
		return userJwt;
	}

	private async Task<Result<JsonWebToken, AppError>> CreateUserJwt(long userId, string email, string name) {
		var resultToken = _jwtManager.GenerateToken(new JwtUser {
			Name = name,
			Email = email,
			Id = userId
		});
		if (resultToken.Error is not null)
			return resultToken.Error;

		var userJwt = resultToken.Value;
		await _tokensRepository.Add(new UserToken {
			UserId = userJwt!.UserId,
			Token = userJwt!.RefreshToken,
			TokenTypeId = (long)Data.Enums.TokenType.RefreshToken,
			ExpiryTime = DateTime.Now.AddSeconds(_jwtManager.RefreshTokenTTLSeconds)
		}).ConfigureAwait(false);
		return userJwt!;
	}
}
