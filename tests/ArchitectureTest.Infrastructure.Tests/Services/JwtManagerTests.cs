using Xunit;
using FluentAssertions;
using System;
using ArchitectureTest.Infrastructure.Services;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Diagnostics;
using ArchitectureTest.Domain.Models.Application;
using ArchitectureTest.TestUtils;
using System.Linq;
using ArchitectureTest.Domain.Errors;
namespace ArchitectureTest.Infrastructure.Tests.Services;

public class JwtManagerTests {
    private readonly JwtManager _systemUnderTest;
    public JwtManagerTests()
    {
        var jwtConfig = new Dictionary<string, string?>{
            {"ConfigData:Jwt:TokenTTLSeconds", "3600"},
            {"ConfigData:Jwt:RefreshTokenTTLHours", "720"},
            {"ConfigData:Jwt:Issuer", "TestIssuer"},
            {"ConfigData:Jwt:Audience", "MyAudience"},
            {"ConfigData:Jwt:Secret", "ThisIsASuperSecretSecurityKeyUsedForMySampleApiServices"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(jwtConfig)
            .Build();
        
        TokenValidationParameters tokenValidationParameters = new() {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration.GetValue<string>("ConfigData:Jwt:Issuer")!,
            ValidAudience = configuration.GetValue<string>("ConfigData:Jwt:Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("ConfigData:Jwt:Secret")!)
            ),
            ClockSkew = Debugger.IsAttached ? TimeSpan.Zero : TimeSpan.FromMinutes(10)
        };

        _systemUnderTest = new(tokenValidationParameters, configuration);
    }

    [Fact]
    public void GenerateToken_WithValidTokenIdentity_ShouldReturnJwt()
    {
        // Arrange
        var tokenIdentity = new UserTokenIdentity {
            UserId = StubData.UserId, Email = StubData.Email, Name = StubData.UserName
        };

        // Act
        var result = _systemUnderTest.GenerateToken(tokenIdentity);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().NotBeNullOrWhiteSpace();
        result.Value!.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [ClassData(typeof(TokenIdentityBadInputData))]
    public void GenerateToken_WithInvalidTokenIdentity_ShouldReturnInputDataError(long userId, string? email, string? name)
    {
        // Arrange
        var tokenIdentity = new UserTokenIdentity {
            UserId = userId, Email = email, Name = name
        };

        // Act
        var result = _systemUnderTest.GenerateToken(tokenIdentity);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCodes.CannotGenerateJwtToken);
    }

    [Fact]
    public void ReadToken_WithValidData_ShouldReturnCorrectIdentity()
    {
        // Arrange
        var tokenIdentity = new UserTokenIdentity {
            UserId = StubData.UserId, Email = StubData.Email, Name = StubData.UserName
        };
        var token = _systemUnderTest.GenerateToken(tokenIdentity).Value!.Token;

        // Act
        var result = _systemUnderTest.ReadToken(token, true);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Identity.Should().BeEquivalentTo(tokenIdentity);
    }

    [Fact]
    public void ReadToken_WithoutUserName_ShouldReturnError()
    {
        // Arrange
        var tokenWithoutUserName = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwiZW1haWwiOiJzeXN0ZW1AdW5kZXJ0ZXN0LmNvbSIsInVuaXF1ZV9uYW1lIjoiIiwibmJmIjoxNzM1MDkwODYzLCJleHAiOjE3MzUwOTQ0NjIsImlhdCI6MTczNTA5MDg2MywiaXNzIjoiVGVzdElzc3VlciIsImF1ZCI6Ik15QXVkaWVuY2UifQ.tuS3_SwrkRNK5zC7R6upDOkae360UIKsfspoiZ3oCPE";

        // Act
        var result = _systemUnderTest.ReadToken(tokenWithoutUserName, false);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCodes.IncompleteJwtTokenData);
    }

    internal class TokenIdentityBadInputData : TheoryData<long, string?, string?>
    {
        public TokenIdentityBadInputData()
        {
            long validUserId = StubData.UserId;
            string validEmail = StubData.Email;
            string validName = StubData.UserName;

            var testParams = new ITestParam[] {
                new TestParam<long> {
                    Name = "UserId",
                    PossibleValues = [validUserId, 0]
                },
                new TestParam<string?> {
                    Name = "Email",
                    PossibleValues = [validEmail, null, string.Empty, " "]
                },
                new TestParam<string?> {
                    Name = "Name",
                    PossibleValues = [validName, null, string.Empty, " "]
                }
            };

            // skip the first and only valid combination
            var combinations = TestCombinationsGenerator.Combine(testParams).Skip(1);
            foreach (var combination in combinations) {
                Add((long) combination[0], combination[1] as string, combination[2] as string);
            }
        }
    }
}
