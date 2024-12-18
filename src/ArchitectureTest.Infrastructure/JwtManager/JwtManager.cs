﻿using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ArchitectureTest.Domain.Services.Infrastructure.JwtManager;

public class JwtManager : IJwtManager {
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IConfiguration _configuration;

    public JwtManager(TokenValidationParameters tokenValidationParameters, IConfiguration configuration) {
        _tokenHandler = new JwtSecurityTokenHandler();
        _tokenValidationParameters = tokenValidationParameters;
        _configuration = configuration;
    }

    public Result<JsonWebToken, AppError> GenerateToken(JwtUser user) {
        if (user.Id <= 0 || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Name))
            return new AppError(ErrorCodes.CannotGenerateJwtToken);

        int tokenTtlSeconds = _configuration.GetValue<int>("ConfigData:Jwt:TokenTTLSeconds");
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            }),
            Expires = DateTime.UtcNow.AddSeconds(tokenTtlSeconds),
            Issuer = _tokenValidationParameters.ValidIssuer,
            Audience = _tokenValidationParameters.ValidAudience,
            SigningCredentials = new SigningCredentials(
                _tokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = _tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = GenerateRefreshToken();
        return new JsonWebToken {
            Token = _tokenHandler.WriteToken(token),
            Email = user.Email,
            RefreshToken = refreshToken,
            UserId = user.Id,
            ExpiresIn = tokenTtlSeconds
        };
    }

    public Result<(JwtUser JwtUser, ClaimsPrincipal Claims), AppError> ReadToken(string token, bool validateLifeTime) {
        _tokenValidationParameters.ValidateLifetime = validateLifeTime;
        var claims = _tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken _);
        
        var emailClaim = claims.FindFirst(ClaimTypes.Email);
        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        var nameClaim = claims.FindFirst(ClaimTypes.Name);

        if (userIdClaim is null || emailClaim is null || nameClaim is null)
            return new AppError(ErrorCodes.IncompleteJwtTokenData);
    
        var user = new JwtUser {
            Email = emailClaim.Value,
            Id = long.Parse(userIdClaim.Value),
            Name = nameClaim.Value
        };
        return (user, claims);
    }

    private string GenerateRefreshToken() {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
