using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Application;
using ArchitectureTest.Domain.Services.Infrastructure;
using ArchitectureTest.TestUtils;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace ArchitectureTest.IntegrationTests.Endpoints;

public abstract class BaseNotesTests
{
  private readonly WebApplicationFactory<Program> _factory;
  private readonly JsonSerializerOptions _serializerOptions = new () {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };
  private readonly string _jwt;

  public BaseNotesTests(WebApplicationFactory<Program> factory)
  {
    _factory = factory;
    _jwt = GenerateJwt();
  }

  [Fact]
  public async Task GetNoteById_WhenEverythingOK_ReturnsNote()
  {
    // Arrange
    var client = _factory.CreateClient();
    var noteId = "1";
    
    client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt}");

    // Act
    var response = await client.GetAsync($"api/notes/{noteId}");

    // Assert
    response.EnsureSuccessStatusCode();
    response.Content.Headers.ContentType!.ToString().Should().Contain(MediaTypeNames.Application.Json);
    var rawBody = await response.Content.ReadAsStringAsync();
    var body = JsonSerializer.Deserialize<NoteDTO>(rawBody, _serializerOptions)!;
    body.Should().NotBeNull();
    body.Id.Should().Be(noteId);
    body.CreationDate.Should().NotBeNull();
  }

  private string GenerateJwt()
  {
    using var scope = _factory.Services.CreateScope();
    var tokenIdentity = new UserTokenIdentity{
      Email = StubData.Email,
      UserId = StubData.UserId,
      Name = StubData.UserName
    };
    var jwtManagerService = scope.ServiceProvider.GetService<IJwtManager>()!;
    var jwt = jwtManagerService!.GenerateToken(tokenIdentity);
    return jwt.Value!.Token;
  }
}

[Collection("AppRunningWithMySql")]
public class MySqlNotesTests(MySqlApplicationFactory factory) : BaseNotesTests(factory)
{
  // [Fact]
  // public Task Debug() => GetNoteById_WhenEverythingOK_ReturnsNote();
}

[Collection("AppRunningWithSqlServer")]
public class SqlServerNotesTests(SqlServerApplicationFactory factory) : BaseNotesTests(factory)
{
//   [Fact]
//   public Task Debug() => GetNoteById_WhenEverythingOK_ReturnsNote();
}
