using ArchitectureTest.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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

namespace ArchitectureTest.Web.Tests.Controllers;

public class NotesControllerTest
{
    private readonly INotesCrudService _mockNotesCrudService;
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly ILogger<NotesController> _mockLogger;

    private readonly NotesController _systemUnderTest;


    public NotesControllerTest()
    {
        _mockNotesCrudService = Substitute.For<INotesCrudService>();
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _mockLogger = Substitute.For<ILogger<NotesController>>();

        var userClaims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, StubData.UserId.ToString()),
            new Claim(ClaimTypes.Email, StubData.Email),
            new Claim(ClaimTypes.Name, StubData.UserName),
        };
        var identity = new ClaimsIdentity(userClaims, "TestAuthType");
    
        var httpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(identity)
        };

        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        _systemUnderTest = new NotesController(
            _mockNotesCrudService, _mockHttpContextAccessor, _mockLogger
        );
    }

    [Fact]
    public async Task GetById_WhenEverythingIsOK_ReturnsNote()
    {
        // Arrange
        var foundNote = BuildNote();
        _mockNotesCrudService.GetById(StubData.NoteId).Returns(foundNote);

        // Act
        var result = await _systemUnderTest.GetById(StubData.NoteId) as ObjectResult;

        // Assert
        await _mockNotesCrudService.Received(1).GetById(StubData.NoteId);
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<NoteDTO>();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        ObjectComparer.JsonCompare(foundNote, result.Value as NoteDTO).Should().BeTrue();
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
        _mockNotesCrudService.GetById(StubData.NoteId).Returns(new AppError(errorCode));

        // Act
        var result = await _systemUnderTest.GetById(StubData.NoteId) as ObjectResult;

        // Assert
        await _mockNotesCrudService.Received(1).GetById(StubData.NoteId);
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
    public async Task GetUserNotes_WhenEverythingIsOK_ReturnsListOfNotes()
    {
        // Arrange
        var foundNotes = new List<NoteDTO> {
            BuildNote(noteId: "1"),
            BuildNote(noteId: "2")
        };
        _mockNotesCrudService.GetUserNotes().Returns(foundNotes);

        // Act
        var result = await _systemUnderTest.GetAll() as ObjectResult;

        // Assert
        await _mockNotesCrudService.Received(1).GetUserNotes();
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<List<NoteDTO>>();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        ObjectComparer.JsonCompare(foundNotes, result.Value as List<NoteDTO>).Should().BeTrue();
    }

    [Theory]
    [InlineData(ErrorCodes.UnknownError, 0)]
    [InlineData(ErrorCodes.EmailAlreadyInUse, 0)]
    [InlineData(ErrorCodes.AuthorizarionMissing, 0)]
    [InlineData(ErrorCodes.AuthorizationFailed, 0)]
    [InlineData("Exception Found!", 1)]
    public async Task GetUserNotes_WhenRepositoryFails_ReturnsError(string errorCode, int loggerCalls)
    {
        // Arrange
        _mockNotesCrudService.GetUserNotes().Returns(new AppError(errorCode));

        // Act
        var result = await _systemUnderTest.GetAll() as ObjectResult;

        // Assert
        await _mockNotesCrudService.Received(1).GetUserNotes();
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
    public async Task Create_WhenEverythingIsOK_ReturnsNote()
    {
        // Arrange
        var inputData = BuildNote(noteId: string.Empty);
        var createdNote = BuildNote();

        _mockNotesCrudService.Create(inputData).Returns((createdNote, createdNote.Id!));
        string path = "/api/notes";
        _mockHttpContextAccessor.HttpContext!.Request.Path = new PathString(path);

        // Act
        var result = await _systemUnderTest.Create(inputData) as CreatedResult;

        // Assert
        await _mockNotesCrudService.Received(1).Create(inputData);
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<NoteDTO>();
        result!.StatusCode.Should().Be(StatusCodes.Status201Created);
        ObjectComparer.JsonCompare(createdNote, result.Value as NoteDTO).Should().BeTrue();
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
        var inputData = BuildNote(noteId: string.Empty);
        _mockNotesCrudService.Create(inputData).Returns(new AppError(errorCode));
        
        // Act
        var result = await _systemUnderTest.Create(inputData) as ObjectResult;

        // Assert
        await _mockNotesCrudService.Received(1).Create(inputData);
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
    public async Task Update_WhenEverythingIsOK_ReturnsNote()
    {
        // Arrange
        var inputData = BuildNote();
        var modifiedNote = BuildNote(title: "title2", content: "content2");
        // domain.Update will always be called validating if entity belongs to user because that is a 
        // behavior of the domain and cannot be changed by user
        _mockNotesCrudService.Update(inputData.Id!, inputData).Returns(modifiedNote);

        // Act
        var result = await _systemUnderTest.Update(inputData.Id!, inputData) as ObjectResult;

        // Assert
        await _mockNotesCrudService.Received(1).Update(inputData.Id!, inputData);
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
        result!.Value.Should().BeOfType<NoteDTO>();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        ObjectComparer.JsonCompare(modifiedNote, result.Value as NoteDTO).Should().BeTrue();
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
        var inputData = BuildNote();
        _mockNotesCrudService.Update(inputData.Id!, inputData).Returns(new AppError(errorCode));

        // Act
        var result = await _systemUnderTest.Update(inputData.Id!, inputData) as ObjectResult;

        // Assert
        await _mockNotesCrudService.Received(1).Update(inputData.Id!, inputData);
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
        _mockNotesCrudService.DeleteById(StubData.NoteId).Returns((AppError) default!);

        // Act
        var result = await _systemUnderTest.DeleteById(StubData.NoteId) as NoContentResult;

        // Assert
        await _mockNotesCrudService.Received(1).DeleteById(StubData.NoteId);
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
        _mockNotesCrudService.DeleteById(StubData.NoteId).Returns(new AppError(errorCode));

        // Act
        var result = await _systemUnderTest.DeleteById(StubData.NoteId) as ObjectResult;

        // Assert
        await _mockNotesCrudService.Received(1).DeleteById(StubData.NoteId);
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

    private NoteDTO BuildNote(
        string noteId = StubData.NoteId, string title = StubData.NoteTitle, string content = StubData.NoteContent,
        string userId = StubData.UserId, DateTime? creationDate = null, DateTime? modificationDate = null
    ) {
        return new NoteDTO {
            Id = noteId,
            Title = title,
            Content = content,
            UserId = userId,
            CreationDate = creationDate ?? StubData.Today,
            ModificationDate = modificationDate ?? StubData.NextWeek
        };
    }
}
