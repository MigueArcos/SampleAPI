using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.StatusCodes;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using ArchitectureTest.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService;
using ArchitectureTest.Web.Services.UserIdentity;

namespace ArchitectureTest.Tests.Controllers {
    public class NotesControllerTest {
        private readonly Mock<NotesCrudService> mockNotesDomain;
        private readonly NotesController notesController;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<IClaimsUserAccesor<JwtUser>> mockClaimsUserAccesor;
        private const long userId = 1, noteId = 10;
        private const string title = "anyTitle", content = "anyContent";
        private const string randomToken = "eyzhdhhdhd.fhfhhf.fggg", randomRefreshToken = "4nyR3fr35hT0k3n";
        private DateTime date1 = DateTime.Parse("2020-01-05"), date2 = DateTime.Parse("2021-01-05");

        public NotesControllerTest() {
            mockUnitOfWork = new Mock<IUnitOfWork>();
            mockClaimsUserAccesor = new Mock<IClaimsUserAccesor<JwtUser>>();
            mockNotesDomain = new Mock<NotesCrudService>(mockUnitOfWork.Object);
            JwtUser defaultUser = new JwtUser {
                Id = userId,
                Name = "anyName",
                Email = "any@anydomain.any"
            };
            mockClaimsUserAccesor.Setup(cUA => cUA.GetUser()).Returns(defaultUser);
            notesController = new NotesController(mockNotesDomain.Object, mockClaimsUserAccesor.Object);
        }

        [Fact]
        public async Task NotesController_GetById_ReturnsNoteDTO() {
            // Arrange
            var outputData = new NoteDTO { Id = noteId, Title = title, Content = content, UserId = userId };
            mockNotesDomain.Setup(nD => nD.GetById(It.IsAny<long>())).ReturnsAsync(outputData);
            // Act
            var result = await notesController.GetById(noteId) as ObjectResult;

            // Assert
            mockNotesDomain.Verify(nD => nD.GetById(It.IsAny<long>()), Times.Once());
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
                mockNotesDomain.Setup(nD => nD.GetById(It.IsAny<long>())).ThrowsAsync(ErrorStatusCode.UnknownError);
            }
            else {
                mockNotesDomain.Setup(nD => nD.GetById(It.IsAny<long>())).ThrowsAsync(new Exception("Any exception message"));
            }

            // Act
            var result = await notesController.GetById(noteId) as ObjectResult;

            // Assert
            mockNotesDomain.Verify(nD => nD.GetById(It.IsAny<long>()), Times.Once());
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
            mockNotesDomain.Setup(nD => nD.GetUserNotes()).ReturnsAsync(outputData);
            // Act
            var result = await notesController.GetAll() as ObjectResult;

            // Assert
            mockNotesDomain.Verify(nD => nD.GetUserNotes(), Times.Once());
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
                mockNotesDomain.Setup(nD => nD.GetUserNotes()).ThrowsAsync(ErrorStatusCode.UnknownError);
            }
            else {
                mockNotesDomain.Setup(nD => nD.GetUserNotes()).ThrowsAsync(new Exception("Any exception message"));
            }

            // Act
            var result = await notesController.GetAll() as ObjectResult;

            // Assert
            mockNotesDomain.Verify(nD => nD.GetUserNotes(), Times.Once());
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
            mockNotesDomain.Setup(nD => nD.Post(It.IsAny<NoteDTO>())).ReturnsAsync(outputData);
            // Act
            var result = await notesController.Post(inputData) as ObjectResult;

            // Assert
            mockNotesDomain.Verify(nD => nD.Post(It.IsAny<NoteDTO>()), Times.Once());
            Assert.NotNull(result);
            Assert.IsType<NoteDTO>(result.Value);
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
                mockNotesDomain.Setup(nD => nD.Post(It.IsAny<NoteDTO>())).ThrowsAsync(ErrorStatusCode.UnknownError);
            }
            else {
                mockNotesDomain.Setup(nD => nD.Post(It.IsAny<NoteDTO>())).ThrowsAsync(new Exception("Any exception message"));
            }
            
            // Act
            var result = await notesController.Post(inputData) as ObjectResult;

            // Assert
            mockNotesDomain.Verify(nD => nD.Post(It.IsAny<NoteDTO>()), Times.Once());
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
            // domain.Put will always be called validating if entity belongs to user because that is a behavior of the domain and cannot be changed by user
            mockNotesDomain.Setup(nD => nD.Put(It.IsAny<long>(), It.IsAny<NoteDTO>())).ReturnsAsync(outputData);
            // Act
            var result = await notesController.Put(noteId, inputData) as ObjectResult;

            // Assert
            mockNotesDomain.Verify(nD => nD.Put(It.IsAny<long>(), It.IsAny<NoteDTO>()), Times.Once());
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
                mockNotesDomain.Setup(nD => nD.Put(It.IsAny<long>(), It.IsAny<NoteDTO>())).ThrowsAsync(ErrorStatusCode.UnknownError);
            }
            else {
                mockNotesDomain.Setup(nD => nD.Put(It.IsAny<long>(), It.IsAny<NoteDTO>())).ThrowsAsync(new Exception("Any exception message"));
            }

            // Act
            var result = await notesController.Put(noteId, inputData) as ObjectResult;

            // Assert
            mockNotesDomain.Verify(nD => nD.Put(It.IsAny<long>(), It.IsAny<NoteDTO>()), Times.Once());
            Assert.NotNull(result);
            Assert.IsType<ErrorDetail>(result.Value);
            Assert.Equal(ErrorMessages.UnknownError, (result.Value as ErrorDetail).Message);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task NotesController_Delete_Returns204NoContent() {
            // Arrange
            // domain.Delete will always be called validating if entity belongs to user because that is a behavior of the domain and cannot be changed by user
            mockNotesDomain.Setup(nD => nD.Delete(It.IsAny<long>())).ReturnsAsync(true);
            // Act
            var result = await notesController.Delete(noteId) as ObjectResult;

            // Assert
            mockNotesDomain.Verify(nD => nD.Delete(It.IsAny<long>()), Times.Once());
            Assert.NotNull(result);
            Assert.Null(result.Value);
            Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesController_Delete_ThrowsUnknownErrorOnUnhandledException(bool useCustomException) {
            // Arrange
            if (useCustomException) {
                mockNotesDomain.Setup(nD => nD.Delete(It.IsAny<long>())).ThrowsAsync(ErrorStatusCode.UnknownError);
            }
            else {
                mockNotesDomain.Setup(nD => nD.Delete(It.IsAny<long>())).ThrowsAsync(new Exception("Any exception message"));
            }

            // Act
            var result = await notesController.Delete(noteId) as ObjectResult;

            // Assert
            mockNotesDomain.Verify(nD => nD.Delete(It.IsAny<long>()), Times.Once());
            Assert.NotNull(result);
            Assert.IsType<ErrorDetail>(result.Value);
            Assert.Equal(ErrorMessages.UnknownError, (result.Value as ErrorDetail).Message);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
    }
}
