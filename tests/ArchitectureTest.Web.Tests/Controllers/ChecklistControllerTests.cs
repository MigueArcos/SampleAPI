using ArchitectureTest.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Security.Claims;
using ArchitectureTest.Web.Configuration;
using Microsoft.Extensions.Logging;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using ArchitectureTest.Domain.Errors;
using NSubstitute;
using ArchitectureTest.TestUtils;
using FluentAssertions;
using ArchitectureTest.Domain.Models;
using AutoMapper;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;
using ArchitectureTest.Domain.Entities;

namespace ArchitectureTest.Web.Tests.Controllers;

public class ChecklistControllerTest
{
    private readonly IChecklistCrudService _mockChecklistCrudService;
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly ILogger<ChecklistController> _mockLogger;
    private readonly IMapper _mapper;

    private readonly ChecklistController _systemUnderTest;


    public ChecklistControllerTest()
    {
        _mockChecklistCrudService = Substitute.For<IChecklistCrudService>();
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _mockLogger = Substitute.For<ILogger<ChecklistController>>();
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<ApplicationModelsMappingProfile>());
        mapperConfig.AssertConfigurationIsValid();

        _mapper = mapperConfig.CreateMapper();

        var userClaims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, StubData.UserId),
            new Claim(ClaimTypes.Email, StubData.Email),
            new Claim(ClaimTypes.Name, StubData.UserName),
        };
        var identity = new ClaimsIdentity(userClaims, "TestAuthType");
    
        var httpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(identity)
        };

        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        _systemUnderTest = new ChecklistController(
            _mockChecklistCrudService, _mockHttpContextAccessor, _mockLogger
        );
    }

    [Fact]
    public async Task GetById_WhenEverythingIsOK_ReturnsChecklist()
    {
        // Arrange
        var foundChecklist = _mapper.Map<ChecklistDTO>(TestDataBuilders.BuildChecklist());
        _mockChecklistCrudService.GetById(StubData.ChecklistId).Returns(foundChecklist);

        // Act
        var result = await _systemUnderTest.GetById(StubData.ChecklistId) as ObjectResult;

        // Assert
        await _mockChecklistCrudService.Received(1).GetById(StubData.ChecklistId);
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<ChecklistDTO>();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        ObjectComparer.JsonCompare(foundChecklist, result.Value as ChecklistDTO).Should().BeTrue();
    }

    [Theory]
    [InlineData(ErrorCodes.UnknownError, 0)]
    [InlineData(ErrorCodes.EmailAlreadyInUse, 0)]
    [InlineData(ErrorCodes.AuthorizarionMissing, 0)]
    [InlineData(ErrorCodes.AuthorizationFailed, 0)]
    [InlineData("Exception Found!", 1)]
    public async Task GetById_WhenRepositoryFails_ReturnsError(string errorCode, int loggerCalls)
    {
        // Arrange
        _mockChecklistCrudService.GetById(StubData.ChecklistId).Returns(new AppError(errorCode));

        // Act
        var result = await _systemUnderTest.GetById(StubData.ChecklistId) as ObjectResult;

        // Assert
        await _mockChecklistCrudService.Received(1).GetById(StubData.ChecklistId);
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<HttpErrorInfo>();
        var expectedErrorInfo = HttpResponses.TryGetErrorInfo(errorCode);
        (result!.Value as HttpErrorInfo)!.ErrorCode.Should().Be(expectedErrorInfo.ErrorCode);
        result!.StatusCode.Should().Be(expectedErrorInfo.HttpStatusCode);

        if (loggerCalls > 0)
            _mockLogger.Received(loggerCalls).LogError(errorCode);
        else
            _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Fact]
    public async Task GetUserChecklists_WhenEverythingIsOK_ReturnsListOfChecklists()
    {
        // Arrange
        var foundChecklists = _mapper.Map<List<ChecklistDTO>>(new List<Checklist> {
            TestDataBuilders.BuildChecklist(checklistId: "1"),
            TestDataBuilders.BuildChecklist(checklistId: "2")
        });
        _mockChecklistCrudService.GetUserChecklists().Returns(foundChecklists);

        // Act
        var result = await _systemUnderTest.GetAll() as ObjectResult;

        // Assert
        await _mockChecklistCrudService.Received(1).GetUserChecklists();
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<List<ChecklistDTO>>();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        ObjectComparer.JsonCompare(foundChecklists, result.Value as List<ChecklistDTO>).Should().BeTrue();
    }

    [Theory]
    [InlineData(ErrorCodes.UnknownError, 0)]
    [InlineData(ErrorCodes.EmailAlreadyInUse, 0)]
    [InlineData(ErrorCodes.AuthorizarionMissing, 0)]
    [InlineData(ErrorCodes.AuthorizationFailed, 0)]
    [InlineData("Exception Found!", 1)]
    public async Task GetUserChecklists_WhenRepositoryFails_ReturnsError(string errorCode, int loggerCalls)
    {
        // Arrange
        _mockChecklistCrudService.GetUserChecklists().Returns(new AppError(errorCode));

        // Act
        var result = await _systemUnderTest.GetAll() as ObjectResult;

        // Assert
        await _mockChecklistCrudService.Received(1).GetUserChecklists();
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<HttpErrorInfo>();
        var expectedErrorInfo = HttpResponses.TryGetErrorInfo(errorCode);
        (result!.Value as HttpErrorInfo)!.ErrorCode.Should().Be(expectedErrorInfo.ErrorCode);
        result!.StatusCode.Should().Be(expectedErrorInfo.HttpStatusCode);

        if (loggerCalls > 0)
            _mockLogger.Received(loggerCalls).LogError(errorCode);
        else
            _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Fact]
    public async Task Create_WhenEverythingIsOK_ReturnsChecklist()
    {
        // Arrange
        var inputData = _mapper.Map<ChecklistDTO>(TestDataBuilders.BuildChecklist(checklistId: string.Empty));
        var createdChecklist = _mapper.Map<ChecklistDTO>(TestDataBuilders.BuildChecklist());

        _mockChecklistCrudService.Create(inputData).Returns((createdChecklist, createdChecklist.Id!));
        string path = "/api/checklist";
        _mockHttpContextAccessor.HttpContext!.Request.Path = new PathString(path);

        // Act
        var result = await _systemUnderTest.Create(inputData) as CreatedResult;

        // Assert
        await _mockChecklistCrudService.Received(1).Create(inputData);
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<ChecklistDTO>();
        result!.StatusCode.Should().Be(StatusCodes.Status201Created);
        ObjectComparer.JsonCompare(createdChecklist, result.Value as ChecklistDTO).Should().BeTrue();
    }

    [Theory]
    [InlineData(ErrorCodes.UnknownError, 0)]
    [InlineData(ErrorCodes.EmailAlreadyInUse, 0)]
    [InlineData(ErrorCodes.AuthorizarionMissing, 0)]
    [InlineData(ErrorCodes.AuthorizationFailed, 0)]
    [InlineData("Exception Found!", 1)]
    public async Task Create_WhenRepositoryFails_ReturnsError(string errorCode, int loggerCalls)
    {
        // Arrange
        var inputData = _mapper.Map<ChecklistDTO>(TestDataBuilders.BuildChecklist(checklistId: string.Empty));
        _mockChecklistCrudService.Create(inputData).Returns(new AppError(errorCode));
        
        // Act
        var result = await _systemUnderTest.Create(inputData) as ObjectResult;

        // Assert
        await _mockChecklistCrudService.Received(1).Create(inputData);
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<HttpErrorInfo>();
        var expectedErrorInfo = HttpResponses.TryGetErrorInfo(errorCode);
        (result!.Value as HttpErrorInfo)!.ErrorCode.Should().Be(expectedErrorInfo.ErrorCode);
        result!.StatusCode.Should().Be(expectedErrorInfo.HttpStatusCode);

        if (loggerCalls > 0)
            _mockLogger.Received(loggerCalls).LogError(errorCode);
        else
            _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Fact]
    public async Task Update_WhenEverythingIsOK_ReturnsChecklist()
    {
        // Arrange
        var inputData = _mapper.Map<ChecklistDTO>(
            TestDataBuilders.BuildUpdateChecklistModel()
        );
        var modifiedChecklist = _mapper.Map<ChecklistDTO>(TestDataBuilders.BuildChecklist(title: "title2"));
        // domain.Update will always be called validating if entity belongs to user because that is a 
        // behavior of the domain and cannot be changed by user
        _mockChecklistCrudService.Update(inputData.Id!, inputData).Returns(modifiedChecklist);

        // Act
        var result = await _systemUnderTest.Update(inputData.Id!, inputData) as ObjectResult;

        // Assert
        await _mockChecklistCrudService.Received(1).Update(inputData.Id!, inputData);
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<ChecklistDTO>();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        ObjectComparer.JsonCompare(modifiedChecklist, result.Value as ChecklistDTO).Should().BeTrue();
    }

    [Theory]
    [InlineData(ErrorCodes.UnknownError, 0)]
    [InlineData(ErrorCodes.EmailAlreadyInUse, 0)]
    [InlineData(ErrorCodes.AuthorizarionMissing, 0)]
    [InlineData(ErrorCodes.AuthorizationFailed, 0)]
    [InlineData("Exception Found!", 1)]
    public async Task Update_WhenRepositoryFails_ReturnsError(string errorCode, int loggerCalls)
    {
        // Arrange
        var inputData = _mapper.Map<ChecklistDTO>(
            TestDataBuilders.BuildUpdateChecklistModel()
        );
        _mockChecklistCrudService.Update(inputData.Id!, inputData).Returns(new AppError(errorCode));

        // Act
        var result = await _systemUnderTest.Update(inputData.Id!, inputData) as ObjectResult;

        // Assert
        await _mockChecklistCrudService.Received(1).Update(inputData.Id!, inputData);
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<HttpErrorInfo>();
        var expectedErrorInfo = HttpResponses.TryGetErrorInfo(errorCode);
        (result!.Value as HttpErrorInfo)!.ErrorCode.Should().Be(expectedErrorInfo.ErrorCode);
        result!.StatusCode.Should().Be(expectedErrorInfo.HttpStatusCode);

        if (loggerCalls > 0)
            _mockLogger.Received(loggerCalls).LogError(errorCode);
        else
            _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Fact]
    public async Task DeleteById_WhenEverythingIsOK_Returns204NoContent()
    {
        // Arrange
        // domain.Delete will always be called validating if entity belongs to user because 
        // that is a behavior of the domain and cannot be changed by user
        _mockChecklistCrudService.DeleteById(StubData.ChecklistId).Returns((AppError) default!);

        // Act
        var result = await _systemUnderTest.DeleteById(StubData.ChecklistId) as NoContentResult;

        // Assert
        await _mockChecklistCrudService.Received(1).DeleteById(StubData.ChecklistId);
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Theory]
    [InlineData(ErrorCodes.UnknownError, 0)]
    [InlineData(ErrorCodes.EmailAlreadyInUse, 0)]
    [InlineData(ErrorCodes.AuthorizarionMissing, 0)]
    [InlineData(ErrorCodes.AuthorizationFailed, 0)]
    [InlineData("Exception Found!", 1)]
    public async Task DeleteById_WhenRepositoryFails_ReturnsError(string errorCode, int loggerCalls)
    {
        // Arrange
        _mockChecklistCrudService.DeleteById(StubData.ChecklistId).Returns(new AppError(errorCode));

        // Act
        var result = await _systemUnderTest.DeleteById(StubData.ChecklistId) as ObjectResult;

        // Assert
        await _mockChecklistCrudService.Received(1).DeleteById(StubData.ChecklistId);
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<HttpErrorInfo>();
        var expectedErrorInfo = HttpResponses.TryGetErrorInfo(errorCode);
        (result!.Value as HttpErrorInfo)!.ErrorCode.Should().Be(expectedErrorInfo.ErrorCode);
        result!.StatusCode.Should().Be(expectedErrorInfo.HttpStatusCode);

        if (loggerCalls > 0)
            _mockLogger.Received(loggerCalls).LogError(errorCode);
        else
            _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }
}
