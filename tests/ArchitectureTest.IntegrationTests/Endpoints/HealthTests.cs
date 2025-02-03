using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Application;
using ArchitectureTest.TestUtils;
using ArchitectureTest.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace ArchitectureTest.IntegrationTests.Endpoints;

public class HealthTests: IClassFixture<CustomWebApplicationFactory> {
  private readonly WebApplicationFactory <Program> _factory;
  private readonly JsonSerializerOptions _serializerOptions = new () {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };

  public HealthTests(CustomWebApplicationFactory factory) {
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

  [Fact]
  public async Task GetNoteById_WhenEverythingOK_ReturnsNote()
  {
    // Arrange
    var client = _factory.CreateClient();
    var noteId = "1";
    client.DefaultRequestHeaders.Add(HeaderNames.Authorization, StubData.ValidOldJwt);

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

  [Fact]
  public async Task GetChecklistById_WhenEverythingOK_ReturnsChecklist()
  {
    // Arrange
    var client = _factory.CreateClient();
    var checklistId = "2";
    client.DefaultRequestHeaders.Add(HeaderNames.Authorization, StubData.ValidOldJwt);

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
    client.DefaultRequestHeaders.Add(HeaderNames.Authorization, StubData.ValidOldJwt);

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
}
