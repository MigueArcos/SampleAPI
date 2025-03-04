using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models.Application;
using ArchitectureTest.Web.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ArchitectureTest.IntegrationTests.Endpoints;

public abstract class BaseLoginTests
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _serializerOptions = new () {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public BaseLoginTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SignIn_WhenEverythingOK_ReturnsJwt()
    {
        // Arrange
        var signInModel = new SignInModel {
            Email = "migue300995@gmail.com",
            Password = "zeusensacion"
        }; // This user always exists because it's created when the container is started
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(signInModel), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("api/login/sign-in", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.ToString().Should().Contain(MediaTypeNames.Application.Json);
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<JsonWebToken>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.Email.Should().Be(signInModel.Email);
        body.Token.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SignIn_WhenUserDoesNotExists_ReturnsError()
    {
        // Arrange
        var signInModel = new SignInModel {
            Email = "someRandomUser@system.com",
            Password = "someRandomPassword"
        };
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(signInModel), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("api/login/sign-in", jsonContent);

        // Assert
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<HttpErrorInfo>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.ErrorCode.Should().Be(ErrorCodes.UserNotFound);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SignIn_WhenInputIsNotValid_ReturnsError()
    {
        // Arrange
        var signInModel = new SignInModel {
            Email = string.Empty,
            Password = string.Empty
        };
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(signInModel), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("api/login/sign-in", jsonContent);

        // Assert
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<BadRequestHttpErrorInfo>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.ErrorCode.Should().Be(ErrorCodes.ValidationsFailed);
        var errorCodesFromValidation = body.Errors!.Select(e => e.ErrorCode);
        List<string> expectedErrors = [ErrorCodes.InvalidEmail, ErrorCodes.InvalidPassword];
        expectedErrors.Intersect(errorCodesFromValidation).Count().Should().Be(expectedErrors.Count);
    }

    [Fact]
    public async Task SignIn_WhenEverythingOKWithCookies_ReturnsJwt()
    {
        // Arrange
        var signInModel = new SignInModel {
            Email = "migue300995@gmail.com",
            Password = "zeusensacion"
        }; // This user always exists because it's created when the container is started
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(signInModel), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppConstants.SaveAuthInCookieHeader, true.ToString());

        // Act
        var response = await client.PostAsync("api/login/sign-in", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.ToString().Should().Contain(MediaTypeNames.Application.Json);
        response.Content.Headers.Contains("Set-Cookie");
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<JsonWebToken>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.Email.Should().Be(signInModel.Email);
        body.Token.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SignUp_WhenEverythingOK_ReturnsJwt()
    {
        // Arrange
        var signUpModel = new SignUpModel {
            Email = "new.user.1@tests.com",
            Password = "P455w0rd",
            UserName = "New User"
        };
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(signUpModel), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("api/login/sign-up", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.ToString().Should().Contain(MediaTypeNames.Application.Json);
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<JsonWebToken>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.Email.Should().Be(signUpModel.Email);
        body.Token.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SignUp_WhenEverythingOKWithCookies_ReturnsJwt()
    {
        // Arrange
        var signUpModel = new SignUpModel {
            Email = "new.user.2@tests.com",
            Password = "P455w0rd",
            UserName = "New User"
        };
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(signUpModel), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("api/login/sign-up", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.ToString().Should().Contain(MediaTypeNames.Application.Json);
        response.Content.Headers.Contains("Set-Cookie");
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<JsonWebToken>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.Email.Should().Be(signUpModel.Email);
        body.Token.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SignUp_WhenInputIsNotValid_ReturnsError()
    {
        // Arrange
        var signUpModel = new SignUpModel {
            Email = "new.user.2@tests.com",
            Password = string.Empty,
            UserName = string.Empty
        };
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(signUpModel), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("api/login/sign-up", jsonContent);

        // Assert
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<BadRequestHttpErrorInfo>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.ErrorCode.Should().Be(ErrorCodes.ValidationsFailed);
        var errorCodesFromValidation = body.Errors!.Select(e => e.ErrorCode);
        List<string> expectedErrors = [ErrorCodes.InvalidPassword, ErrorCodes.InvalidUserName];
        expectedErrors.Intersect(errorCodesFromValidation).Count().Should().Be(expectedErrors.Count);
    }

    [Fact]
    public async Task SignUp_WhenEmailAlreadyInUser_ReturnsError()
    {
        // Arrange
        var signUpModel = new SignUpModel {
            Email = "migue300995@gmail.com",
            Password = "P455w0rd",
            UserName = "New User"
        };
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(signUpModel), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("api/login/sign-up", jsonContent);

        // Assert
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<HttpErrorInfo>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.ErrorCode.Should().Be(ErrorCodes.EmailAlreadyInUse);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}

[Collection("AppRunningWithMySql")]
public class MySqlLoginTests(MySqlApplicationFactory factory) : BaseLoginTests(factory)
{
}

[Collection("AppRunningWithSqlServer")]
public class SqlServerHealthLoginTests(SqlServerApplicationFactory factory) : BaseLoginTests(factory)
{
    [Fact]
    public Task Debug() => SignIn_WhenEverythingOKWithCookies_ReturnsJwt();
}
