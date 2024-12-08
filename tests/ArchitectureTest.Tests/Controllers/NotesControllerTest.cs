using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.StatusCodes;
using ArchitectureTest.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Security.Claims;
using ArchitectureTest.Domain.ServicesLayer.EntityCrudService.Contracts;

namespace ArchitectureTest.Tests.Controllers;

public class NotesControllerTest {
    private readonly Mock<INotesCrudService> mockNotesCrudService;
    private readonly NotesController notesController;
    private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;

    private const long userId = 1, noteId = 10;
    private const string title = "anyTitle", content = "anyContent";
    private const string randomToken = "eyzhdhhdhd.fhfhhf.fggg", randomRefreshToken = "4nyR3fr35hT0k3n";
    private DateTime date1 = DateTime.Parse("2020-01-05"), date2 = DateTime.Parse("2021-01-05");

    public NotesControllerTest() {
        mockNotesCrudService = new Mock<INotesCrudService>();
        mockNotesCrudService.SetupAllProperties();

        mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var userClaims = new List<Claim> {
            new Claim(ClaimTypes.Name, "anyName"),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "email@domain.any"),
        };
        var identity = new ClaimsIdentity(userClaims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(identity)
        };
        mockHttpContextAccessor.SetupGet(hCA => hCA.HttpContext).Returns(httpContext);
        notesController = new NotesController(mockNotesCrudService.Object, mockHttpContextAccessor.Object);
    }

    [Fact]
    public async Task NotesController_GetById_ReturnsNoteDTO() {
        // Arrange
        var outputData = new NoteDTO { Id = noteId, Title = title, Content = content, UserId = userId };
        mockNotesCrudService.Setup(nD => nD.GetById(It.IsAny<long>())).ReturnsAsync(outputData);
        // Act
        var result = await notesController.GetById(noteId) as ObjectResult;

        // Assert
        mockNotesCrudService.Verify(nD => nD.GetById(It.IsAny<long>()), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<NoteDTO>(result.Value);
        Assert.Equal(noteId, (result.Value as NoteDTO).Id);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesController_GetById_ThrowsUnknownErrorOnUnhandledException(bool useCustomException) {
        // Arrange
        if (useCustomException) {
            mockNotesCrudService.Setup(nD => nD.GetById(It.IsAny<long>())).ThrowsAsync(ErrorStatusCode.UnknownError);
        }
        else {
            mockNotesCrudService.Setup(nD => nD.GetById(It.IsAny<long>())).ThrowsAsync(new Exception("Any exception message"));
        }

        // Act
        var result = await notesController.GetById(noteId) as ObjectResult;

        // Assert
        mockNotesCrudService.Verify(nD => nD.GetById(It.IsAny<long>()), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<ErrorDetail>(result.Value);
        Assert.Equal(ErrorMessages.UnknownError, (result.Value as ErrorDetail).Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task NotesController_GetAll_ReturnsListOfNotes() {
        // Arrange
        var outputData = new List<NoteDTO> {
            new NoteDTO { Id = noteId, Title = title, Content = content, UserId = userId }
        };
        mockNotesCrudService.Setup(nD => nD.GetUserNotes()).ReturnsAsync(outputData);
        // Act
        var result = await notesController.GetAll() as ObjectResult;

        // Assert
        mockNotesCrudService.Verify(nD => nD.GetUserNotes(), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<List<NoteDTO>>(result.Value);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesController_GetAll_ThrowsUnknownErrorOnUnhandledException(bool useCustomException) {
        // Arrange
        if (useCustomException) {
            mockNotesCrudService.Setup(nD => nD.GetUserNotes()).ThrowsAsync(ErrorStatusCode.UnknownError);
        }
        else {
            mockNotesCrudService.Setup(nD => nD.GetUserNotes()).ThrowsAsync(new Exception("Any exception message"));
        }

        // Act
        var result = await notesController.GetAll() as ObjectResult;

        // Assert
        mockNotesCrudService.Verify(nD => nD.GetUserNotes(), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<ErrorDetail>(result.Value);
        Assert.Equal(ErrorMessages.UnknownError, (result.Value as ErrorDetail).Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task NotesController_Post_ReturnsNoteDTO() {
        // Arrange
        var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
        var outputData = new NoteDTO { Id = noteId, Title = title, Content = content, UserId = userId };
        mockNotesCrudService.Setup(nD => nD.Post(It.IsAny<NoteDTO>())).ReturnsAsync(outputData);
        
        string path = "/api/notes";
        mockHttpContextAccessor.Object.HttpContext.Request.Path = new PathString(path);
        // Act
        var result = await notesController.Post(inputData) as CreatedResult;

        // Assert
        mockNotesCrudService.Verify(nD => nD.Post(It.IsAny<NoteDTO>()), Times.Once());
        Assert.IsType<CreatedResult>(result);
        Assert.NotNull(result);
        Assert.IsType<NoteDTO>(result.Value);
        Assert.Equal($"{path}/{(result.Value as NoteDTO).Id}", result.Location);
        Assert.Equal(noteId, (result.Value as NoteDTO).Id);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesController_Post_ThrowsUnknownErrorOnUnhandledException(bool useCustomException) {
        // Arrange
        var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
        if (useCustomException) {
            mockNotesCrudService.Setup(nD => nD.Post(It.IsAny<NoteDTO>())).ThrowsAsync(ErrorStatusCode.UnknownError);
        }
        else {
            mockNotesCrudService.Setup(nD => nD.Post(It.IsAny<NoteDTO>())).ThrowsAsync(new Exception("Any exception message"));
        }
        
        // Act
        var result = await notesController.Post(inputData) as ObjectResult;

        // Assert
        mockNotesCrudService.Verify(nD => nD.Post(It.IsAny<NoteDTO>()), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<ErrorDetail>(result.Value);
        Assert.Equal(ErrorMessages.UnknownError, (result.Value as ErrorDetail).Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task NotesController_Put_ReturnsNoteDTO() {
        // Arrange
        var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
        var outputData = new NoteDTO { Id = noteId, Title = title, Content = content, UserId = userId };
        // domain.Put will always be called validating if entity belongs to user because that is a 
        // behavior of the domain and cannot be changed by user
        mockNotesCrudService.Setup(nD => nD.Put(It.IsAny<long>(), It.IsAny<NoteDTO>())).ReturnsAsync(outputData);
        // Act
        var result = await notesController.Put(noteId, inputData) as ObjectResult;

        // Assert
        mockNotesCrudService.Verify(nD => nD.Put(It.IsAny<long>(), It.IsAny<NoteDTO>()), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<NoteDTO>(result.Value);
        Assert.Equal(noteId, (result.Value as NoteDTO).Id);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesController_Put_ThrowsUnknownErrorOnUnhandledException(bool useCustomException) {
        // Arrange
        var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
        if (useCustomException) {
            mockNotesCrudService
                .Setup(nD => nD.Put(It.IsAny<long>(), It.IsAny<NoteDTO>()))
                .ThrowsAsync(ErrorStatusCode.UnknownError);
        }
        else {
            mockNotesCrudService
                .Setup(nD => nD.Put(It.IsAny<long>(), It.IsAny<NoteDTO>()))
                .ThrowsAsync(new Exception("Any exception message"));
        }

        // Act
        var result = await notesController.Put(noteId, inputData) as ObjectResult;

        // Assert
        mockNotesCrudService.Verify(nD => nD.Put(It.IsAny<long>(), It.IsAny<NoteDTO>()), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<ErrorDetail>(result.Value);
        Assert.Equal(ErrorMessages.UnknownError, (result.Value as ErrorDetail).Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task NotesController_Delete_Returns204NoContent() {
        // Arrange
        // domain.Delete will always be called validating if entity belongs to user because 
        // that is a behavior of the domain and cannot be changed by user
        mockNotesCrudService.Setup(nD => nD.Delete(It.IsAny<long>())).ReturnsAsync(true);
        // Act
        var result = await notesController.Delete(noteId) as NoContentResult;

        // Assert
        mockNotesCrudService.Verify(nD => nD.Delete(It.IsAny<long>()), Times.Once());
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesController_Delete_ThrowsUnknownErrorOnUnhandledException(bool useCustomException) {
        // Arrange
        if (useCustomException) {
            mockNotesCrudService.Setup(nD => nD.Delete(It.IsAny<long>())).ThrowsAsync(ErrorStatusCode.UnknownError);
        }
        else {
            mockNotesCrudService.Setup(nD => nD.Delete(It.IsAny<long>())).ThrowsAsync(new Exception("Any exception message"));
        }

        // Act
        var result = await notesController.Delete(noteId) as ObjectResult;

        // Assert
        mockNotesCrudService.Verify(nD => nD.Delete(It.IsAny<long>()), Times.Once());
        Assert.NotNull(result);
        Assert.IsType<ErrorDetail>(result.Value);
        Assert.Equal(ErrorMessages.UnknownError, (result.Value as ErrorDetail).Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
    }
}
