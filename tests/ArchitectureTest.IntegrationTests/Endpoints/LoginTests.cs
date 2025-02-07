using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Models.Application;
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
    };
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
}

[Collection("AppRunningWithMySql")]
public class MySqlLoginTests(MySqlApplicationFactory factory) : BaseLoginTests(factory)
{
}

[Collection("AppRunningWithSqlServer")]
public class SqlServerHealthLoginTests(SqlServerApplicationFactory factory) : BaseLoginTests(factory)
{
}
