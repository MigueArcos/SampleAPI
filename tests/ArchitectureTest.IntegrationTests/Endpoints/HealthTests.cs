using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using ArchitectureTest.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ArchitectureTest.IntegrationTests.Endpoints;

public abstract class BaseHealthTests
{
  private readonly WebApplicationFactory<Program> _factory;
  private readonly JsonSerializerOptions _serializerOptions = new () {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };

  public BaseHealthTests(WebApplicationFactory<Program> factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task Health_WhenHostCreatedSuccessfully_ReturnsOK()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("api/health");

    // Assert
    response.EnsureSuccessStatusCode();
    response.Content.Headers.ContentType!.ToString().Should().Contain(MediaTypeNames.Application.Json);
    var rawBody = await response.Content.ReadAsStringAsync();
    var body = JsonSerializer.Deserialize<HealthController.HealthInfo>(rawBody, _serializerOptions)!;
    body.Should().NotBeNull();
  }
}

[Collection("AppRunningWithMySql")]
public class MySqlHealthTests(MySqlApplicationFactory factory) : BaseHealthTests(factory)
{
}

[Collection("AppRunningWithSqlServer")]
public class SqlServerHealthTests(SqlServerApplicationFactory factory) : BaseHealthTests(factory)
{
}
