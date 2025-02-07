using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Application;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using ArchitectureTest.Domain.Services.Infrastructure;
using ArchitectureTest.TestUtils;
using ArchitectureTest.Web.Configuration;
using AutoMapper;
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
    private readonly JsonWebToken _jwt;
    private readonly IMapper _mapper;
    private readonly Random _random = new();

    public BaseChecklistsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<ApplicationModelsMappingProfile>());
        mapperConfig.AssertConfigurationIsValid();
        _mapper = mapperConfig.CreateMapper();

        _jwt = GenerateJwt();
    }

    [Fact]
    public async Task GetChecklistById_WhenEverythingOK_ReturnsChecklist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var checklistId = "2";
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt.Token}");

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
    public async Task GetChecklistById_WhenUserIsUnauthorized_ReturnsError()
    {
        // Arrange
        var client = _factory.CreateClient();
        var checklistId = "2";
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {StubData.ValidOldJwt}");

        // Act
        var response = await client.GetAsync($"api/checklist/{checklistId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetChecklistById_WhenEntityNotFound_ReturnsError()
    {
        // Arrange
        var client = _factory.CreateClient();
        var checklistId = "SomethingInexistent";
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt.Token}");

        // Act
        var response = await client.GetAsync($"api/checklist/{checklistId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<HttpErrorInfo>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.ErrorCode.Should().Be(ErrorCodes.EntityNotFound);
    }

    [Fact]
    public async Task GetUserChecklists_WhenEverythingOK_ReturnsListOfChecklist()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt.Token}");

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

    [Fact]
    public async Task GetUserChecklists_WhenUserIsUnauthorized_ReturnsListOfChecklist()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {StubData.ValidOldJwt}");

        // Act
        var response = await client.GetAsync($"api/checklist");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WhenEverythingOK_ReturnsSuccess()
    {
        // Arrange
        var inputData = BuildChecklist(checklistId: null!, userId: _jwt.UserId);
        var mappedInputData = _mapper.Map<ChecklistDTO>(inputData);
        var client = _factory.CreateClient();
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(mappedInputData), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt.Token}");

        // Act
        var response = await client.PostAsync($"api/checklist", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        response.Content.Headers.ContentType!.ToString().Should().Contain(MediaTypeNames.Application.Json);
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<ChecklistDTO>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        Guid.TryParse(body.Id, out _).Should().BeTrue();
        // body.UserId.Should().Be(_jwt.UserId);
    }

    [Fact]
    public async Task Create_WhenUserIsUnauthorized_ReturnsError()
    {
        // Arrange
        var inputData = BuildChecklist(checklistId: null!, userId: _jwt.UserId);
        var mappedInputData = _mapper.Map<ChecklistDTO>(inputData);
        var client = _factory.CreateClient();
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(mappedInputData), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {StubData.ValidOldJwt}");

        // Act
        var response = await client.PostAsync($"api/checklist", jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WhenThereIsAValidationError_ReturnsError()
    {
        // Arrange
        var inputData = BuildChecklist(checklistId: null!, userId: _jwt.UserId, title: string.Empty);
        var mappedInputData = _mapper.Map<ChecklistDTO>(inputData);
        var client = _factory.CreateClient();
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(mappedInputData), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt.Token}");

        // Act
        var response = await client.PostAsync($"api/checklist", jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<HttpErrorInfo>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.ErrorCode.Should().Be(ErrorCodes.ChecklistTitleNotFound);
        // body.UserId.Should().Be(_jwt.UserId);
    }

    [Fact]
    public async Task Update_WhenEverythingOK_ReturnsSuccess()
    {
        // Arrange
        var checklistId = "2"; // this always exists when the database is created
        var newTitle = "SomethingNew";
        var checklistToUpdate = await GetChecklistFromDatabase(checklistId);
        var oldFlattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(
            _mapper.Map<List<ChecklistDetail>>(checklistToUpdate.Details)
        );
        // take n random items to update and also n random items to delete from getByIdChecklist
        var (detailsToUpdate, detailsToDelete) = PickRandomDetails(oldFlattenedDetails);
        detailsToUpdate.ForEach(d => {
            d.TaskName = StubData.CreateRandomString();
            d.Status = RandomBool();
        });
        var inputData = BuildUpdateChecklistModel(
            checklistId: checklistId, detailsToUpdate: detailsToUpdate, detailsToDelete: detailsToDelete,
            title: newTitle, userId: _jwt.UserId
        );
        var client = _factory.CreateClient();
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(inputData), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt.Token}");

        // Act
        var response = await client.PutAsync($"api/checklist/{checklistId}", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType!.ToString().Should().Contain(MediaTypeNames.Application.Json);
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<ChecklistDTO>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.Id.Should().Be(checklistId);
        // body.ModificationDate.Should().NotBeNull();
        body.Title.Should().Be(newTitle);
        body.UserId.Should().Be(_jwt.UserId);
    }

    [Fact]
    public async Task Update_WhenUserIsUnauthorized_ReturnsError()
    {
        // Arrange
        var checklistId = "2"; // this always exists when the database is created
        var newTitle = "SomethingNew";
        var checklistToUpdate = await GetChecklistFromDatabase(checklistId);
        var oldFlattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(
            _mapper.Map<List<ChecklistDetail>>(checklistToUpdate.Details)
        );
        // take n random items to update and also n random items to delete from getByIdChecklist
        var (detailsToUpdate, detailsToDelete) = PickRandomDetails(oldFlattenedDetails);
        detailsToUpdate.ForEach(d => {
            d.TaskName = StubData.CreateRandomString();
            d.Status = RandomBool();
        });
        var inputData = BuildUpdateChecklistModel(
            checklistId: checklistId, detailsToUpdate: detailsToUpdate, detailsToDelete: detailsToDelete,
            title: newTitle, userId: _jwt.UserId
        );
        var client = _factory.CreateClient();
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(inputData), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {StubData.ValidOldJwt}");

        // Act
        var response = await client.PutAsync($"api/checklist/{checklistId}", jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_WhenThereIsAValidationError_ReturnsError()
    {
        // Arrange
        var checklistId = "2"; // this always exists when the database is created
        var checklistToUpdate = await GetChecklistFromDatabase(checklistId);
        var oldFlattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(
            _mapper.Map<List<ChecklistDetail>>(checklistToUpdate.Details)
        );
        // take n random items to update and also n random items to delete from getByIdChecklist
        var (detailsToUpdate, detailsToDelete) = PickRandomDetails(oldFlattenedDetails);
        detailsToUpdate.ForEach(d => {
            d.TaskName = StubData.CreateRandomString();
            d.Status = RandomBool();
        });
        var inputData = BuildUpdateChecklistModel(
            checklistId: checklistId, detailsToUpdate: detailsToUpdate, detailsToDelete: detailsToDelete,
            title: string.Empty, userId: _jwt.UserId
        );
        var client = _factory.CreateClient();
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(inputData), Encoding.UTF8, MediaTypeNames.Application.Json
        );
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt.Token}");

        // Act
        var response = await client.PutAsync($"api/checklist/{checklistId}", jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<HttpErrorInfo>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.ErrorCode.Should().Be(ErrorCodes.ChecklistTitleNotFound);
    }

    [Fact]
    public async Task DeleteById_WhenEverythingOK_ReturnsChecklist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var checklistId = "1"; // this always exists when the database is created
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt.Token}");

        // Act
        var response = await client.DeleteAsync($"api/checklist/{checklistId}");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteById_WhenUserIsUnauthorized_ReturnsError()
    {
        // Arrange
        var client = _factory.CreateClient();
        var checklistId = "1";
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {StubData.ValidOldJwt}");

        // Act
        var response = await client.DeleteAsync($"api/checklist/{checklistId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteById_WhenEntityNotFound_ReturnsError()
    {
        // Arrange
        var client = _factory.CreateClient();
        var checklistId = "SomethingInexistent";
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {_jwt.Token}");

        // Act
        var response = await client.DeleteAsync($"api/checklist/{checklistId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        var rawBody = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<HttpErrorInfo>(rawBody, _serializerOptions)!;
        body.Should().NotBeNull();
        body.ErrorCode.Should().Be(ErrorCodes.EntityNotFound);
    }

    private JsonWebToken GenerateJwt()
    {
        using var scope = _factory.Services.CreateScope();
        var tokenIdentity = new UserTokenIdentity{
            Email = StubData.Email,
            UserId = StubData.UserId,
            Name = StubData.UserName
        };
        var jwtManagerService = scope.ServiceProvider.GetService<IJwtManager>()!;
        var jwt = jwtManagerService!.GenerateToken(tokenIdentity);
        return jwt.Value!;
    }

    private async Task<ChecklistDTO> GetChecklistFromDatabase(string checklistId)
    {
        using var scope = _factory.Services.CreateScope();
        var checklistService = scope.ServiceProvider.GetService<IChecklistCrudService>()!;
        var checklist = await checklistService!.GetById(checklistId);
        return checklist.Value!;
    }

    private Checklist BuildChecklist(
        string checklistId = StubData.ChecklistId, string userId = StubData.UserId, string title = StubData.ChecklistTitle,
        DateTime? creationDate = null, DateTime? modificationDate = null, List<ChecklistDetail>? details = null
    ) {
        details ??= BuildRandomDetails(checklistId);

        return new Checklist {
            Id = checklistId,
            UserId = userId,
            Title = title,
            Details = details,
            CreationDate = creationDate ?? StubData.Today,
            ModificationDate = modificationDate ?? StubData.NextWeek
        };
    }

    private UpdateChecklistDTO BuildUpdateChecklistModel(
        string checklistId = StubData.ChecklistId, string userId = StubData.UserId,
        string title = StubData.ChecklistTitle, List<ChecklistDetail>? detailsToAdd = null,
        List<ChecklistDetail>? detailsToUpdate = null, List<string>? detailsToDelete = null
    ) {
        detailsToAdd ??= BuildRandomDetails(checklistId);
        // detailsToUpdate ??= BuildRandomDetails(checklistId);
        // detailsToDelete ??= 

        return new UpdateChecklistDTO {
            Id = checklistId,
            UserId = userId,
            Title = title,
            DetailsToAdd = _mapper.Map<List<ChecklistDetailDTO>>(detailsToAdd),
            DetailsToUpdate = _mapper.Map<List<ChecklistDetailDTO>>(detailsToUpdate),
            DetailsToDelete = detailsToDelete
        };
    }

    private List<ChecklistDetail>? BuildRandomDetails(string checklistId, int depth = 0, string? parentDetailId = null)
    {
        var details = new List<ChecklistDetail>();
        int detailsNumber = _random.Next(depth == 0 ? 1 : 0, 5 - depth);
        for (int i = 0; i < detailsNumber; i++)
        {
            var detail = BuildChecklistDetail(
                checklistId: checklistId, taskName: StubData.CreateRandomString(), parentDetailId: parentDetailId
            );
            detail.SubItems = BuildRandomDetails(checklistId, depth + 1, detail.Id);
            // detail.SubItems = [];
            details.Add(detail);
        }
        return details;
    }

    private ChecklistDetail BuildChecklistDetail(
        string? detailId = null, string checklistId = StubData.ChecklistId, string taskName = StubData.ChecklistTaskName,
        string? parentDetailId = null, bool status = true, DateTime? creationDate = null, DateTime? modificationDate = null
    ) {
        return new ChecklistDetail {
            Id = string.IsNullOrWhiteSpace(detailId) ? Guid.CreateVersion7().ToString("N") : detailId,
            ChecklistId = checklistId,
            TaskName = taskName,
            ParentDetailId = parentDetailId,
            Status = status,
            CreationDate = creationDate ?? StubData.Today,
            ModificationDate = modificationDate ?? StubData.NextWeek
        };
    }

    private bool RandomBool() => _random.NextDouble() >= 0.5;

    private (List<ChecklistDetail> DetailToUpdate, List<string> DetailsToDelete) PickRandomDetails(
        List<ChecklistDetail> flattenedDetails
    ){
        var detailsToUpdate = new List<ChecklistDetail>();
        var detailsToDelete = new List<string>();

        flattenedDetails.ForEach(detail => {
            if (RandomBool())
                detailsToUpdate.Add(detail);
            if (RandomBool())
                detailsToDelete.Add(detail.Id);
        });
        
        return (detailsToUpdate, detailsToDelete);
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
    // public Task Debug() => Update_WhenEverythingOK_ReturnsSuccess();
}
