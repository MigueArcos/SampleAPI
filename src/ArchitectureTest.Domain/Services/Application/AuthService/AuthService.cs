using System;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services.Infrastructure;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Application;

namespace ArchitectureTest.Domain.Services.Application.AuthService;

public class AuthService : IAuthService {
    private readonly IDomainRepository<UserEntity> _usersRepository;
    private readonly IDomainRepository<UserTokenEntity> _tokensRepository;
    private readonly IJwtManager _jwtManager;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        IDomainUnitOfWork unitOfWork, IJwtManager jwtManager, IPasswordHasher passwordHasher, IConfiguration configuration
    ) {
        _jwtManager = jwtManager;
        _passwordHasher = passwordHasher;
        _usersRepository = unitOfWork.Repository<UserEntity>();
        _tokensRepository = unitOfWork.Repository<UserTokenEntity>();
        _configuration = configuration;
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

        var newUser = await _usersRepository.Add(new UserEntity {
            Name = signUpModel.UserName,
            Email = signUpModel.Email,
            Password = _passwordHasher.Hash(signUpModel.Password),
            CreationDate = DateTime.Now
        }).ConfigureAwait(false);

        var userJwt = await CreateUserJwt(newUser.Id, newUser.Email, newUser.Name).ConfigureAwait(false);
        return userJwt;
    }

    public async Task<Result<(JsonWebToken Token, ClaimsPrincipal Claims), AppError>> ExchangeOldTokensForNewToken(
        string token, string refreshToken
    ) {
        // validate refreshToken in DB
        var refreshTokenSearch = await _tokensRepository.FindSingle(t => t.Token == refreshToken).ConfigureAwait(false);
        if (refreshTokenSearch == null) 
            return new AppError(ErrorCodes.RefreshTokenExpired);

        var resultJwtRead = _jwtManager.ReadToken(token, false);

        if (resultJwtRead.Error != null) 
            return resultJwtRead.Error;
        
        var jwtResult = resultJwtRead.Value;

        var newTokenResult = _jwtManager.GenerateToken(jwtResult.Identity);

        if (newTokenResult.Error != null)
            return newTokenResult.Error;

        var newToken = newTokenResult.Value;
        // Delete previous token from database
        await _tokensRepository.DeleteById(refreshTokenSearch.Id).ConfigureAwait(false);

        // Create a new token in Database
        await _tokensRepository.Add(new UserTokenEntity {
            UserId = newToken!.UserId,
            Token = newToken.RefreshToken,
            TokenTypeId = (long)Enums.TokenType.RefreshToken,
            ExpiryTime = DateTime.Now.AddHours(_configuration.GetValue<int>("ConfigData:Jwt:RefreshTokenTTLHours"))
        }).ConfigureAwait(false);
        return (newToken, jwtResult.Claims);
    }

    private async Task<Result<JsonWebToken, AppError>> CreateUserJwt(long userId, string email, string? name) {
        var resultToken = _jwtManager.GenerateToken(new UserTokenIdentity {
            Name = name,
            Email = email,
            UserId = userId
        });
        if (resultToken.Error is not null)
            return resultToken.Error;

        var userJwt = resultToken.Value;
        await _tokensRepository.Add(new UserTokenEntity {
            UserId = userJwt!.UserId,
            Token = userJwt!.RefreshToken,
            TokenTypeId = (long)Enums.TokenType.RefreshToken,
            ExpiryTime = DateTime.Now.AddHours(_configuration.GetValue<int>("ConfigData:Jwt:RefreshTokenTTLHours"))
        }).ConfigureAwait(false);
        return userJwt!;
    }
}
