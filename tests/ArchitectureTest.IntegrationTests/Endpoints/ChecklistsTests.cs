using System.Collections.Generic;
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

public abstract class BaseChecklistsTests
{
  private readonly WebApplicationFactory<Program> _factory;
  private readonly JsonSerializerOptions _serializerOptions = new () {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };
  private readonly string _jwt;

  public BaseChecklistsTests(WebApplicationFactory<Program> factory)
  {
    _factory = factory;
    _jwt = GenerateJwt();
  }

  [Fact]
  public async Task GetChecklistById_WhenEverythingOK_ReturnsChecklist()
  {
    // Arrange
    var client = _factory.CreateClient();
    var checklistId = "2";
    client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt}");

    // Act
    var response = await client.GetAsync($"api/checklist/{checklistId}");

    // Assert
    response.EnsureSuccessStatusCode();
    response.Content.Headers.ContentType!.ToString().Should().Contain(MediaTypeNames.Application.Json);
    var rawBody = await response.Content.ReadAsStringAsync();
    var body = JsonSerializer.Deserialize<ChecklistDTO>(rawBody, _serializerOptions)!;
    body.Should().NotBeNull();
    body.Id.Should().Be(checklistId);
    body.CreationDate.Should().NotBeNull();
  }

  [Fact]
  public async Task GetUserChecklists_WhenEverythingOK_ReturnsListOfChecklist()
  {
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt}");

    // Act
    var response = await client.GetAsync($"api/checklist");

    // Assert
    response.EnsureSuccessStatusCode();
    response.Content.Headers.ContentType!.ToString().Should().Contain(MediaTypeNames.Application.Json);
    var rawBody = await response.Content.ReadAsStringAsync();
    var body = JsonSerializer.Deserialize<List<ChecklistDTO>>(rawBody, _serializerOptions)!;
    body.Should().NotBeNullOrEmpty();
    body.ForEach(c => {
      // Guid.TryParse(c.Id, out _).Should().BeTrue();
      c.Id.Should().NotBeNullOrWhiteSpace();
      c.CreationDate.Should().NotBeNull();
    });
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
public class MySqlChecklistsTests(MySqlApplicationFactory factory) : BaseChecklistsTests(factory)
{
  // [Fact]
  // public Task Debug() => GetNoteById_WhenEverythingOK_ReturnsNote();
}

[Collection("AppRunningWithSqlServer")]
public class SqlServerChecklistsTests(SqlServerApplicationFactory factory) : BaseChecklistsTests(factory)
{
  // [Fact]
  // public Task Debug() => GetNoteById_WhenEverythingOK_ReturnsNote();
}
