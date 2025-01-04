using System;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services.Infrastructure;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Application;
using Microsoft.Extensions.Logging;

namespace ArchitectureTest.Domain.Services.Application.AuthService;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _usersRepository;
    private readonly IRepository<UserToken> _tokensRepository;
    private readonly IJwtManager _jwtManager;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork, IJwtManager jwtManager, IPasswordHasher passwordHasher,
        IConfiguration configuration, ILogger<AuthService> logger
    ) {
        _jwtManager = jwtManager;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _usersRepository = unitOfWork.Repository<User>();
        _tokensRepository = unitOfWork.Repository<UserToken>();
        _configuration = configuration;
        _logger = logger;
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

        var newUser = new User {
            Id = Guid.CreateVersion7().ToString("N"),
            Name = signUpModel.UserName,
            Email = signUpModel.Email,
            Password = _passwordHasher.Hash(signUpModel.Password),
            CreationDate = DateTime.Now
        };
        await _usersRepository.Create(newUser).ConfigureAwait(false);

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

        var transactionFunc = () => TransactionallyRefreshTokens(refreshTokenSearch.Id, jwtResult.Identity);
        var transactionResult = await _unitOfWork.RunAsyncTransaction(transactionFunc, _logger).ConfigureAwait(false);

        if (transactionResult.Error != null) 
            return transactionResult.Error;

        return (transactionResult.Value!, jwtResult.Claims);
    }

    private async Task<Result<JsonWebToken, AppError>> TransactionallyRefreshTokens(
        string oldRefreshTokenId, UserTokenIdentity tokenIdentity
    ){
        // Delete previous token from database
        await _tokensRepository.DeleteById(oldRefreshTokenId, autoSave: false).ConfigureAwait(false);

        // Create a new token in Database
        return await CreateUserJwt(tokenIdentity.UserId, tokenIdentity.Email, tokenIdentity.Name, autoSave: false)
            .ConfigureAwait(false);
    }

    private async Task<Result<JsonWebToken, AppError>> CreateUserJwt(
        string userId, string email, string? name, bool autoSave = true
    ) {
        var resultToken = _jwtManager.GenerateToken(new UserTokenIdentity {
            Name = name,
            Email = email,
            UserId = userId
        });
        if (resultToken.Error is not null)
            return resultToken.Error;

        var userJwt = resultToken.Value!;
        await _tokensRepository.Create(new UserToken {
            Id = Guid.CreateVersion7().ToString("N"),
            UserId = userJwt.UserId,
            Token = userJwt.RefreshToken,
            TokenTypeId = $"{(int) Enums.TokenType.RefreshToken}",
            ExpiryTime = DateTime.Now.AddHours(_configuration.GetValue<int>("ConfigData:Jwt:RefreshTokenTTLHours"))
        }, autoSave).ConfigureAwait(false);
        return userJwt;
    }
}
