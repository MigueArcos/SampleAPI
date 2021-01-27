using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.StatusCodes;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using ArchitectureTest.Tests.Shared.Mocks;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService;

namespace ArchitectureTest.Tests.Domain.Services {
    public class NotesCrudServiceTests {
        private readonly MockRepository<Note> mockNotesRepo;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly NotesCrudService notesCrudService;
        private const long userId = 1, noteId = 10;
        private const string title = "anyTitle", content = "anyContent";
        private const string randomToken = "eyzhdhhdhd.fhfhhf.fggg", randomRefreshToken = "4nyR3fr35hT0k3n";
        private DateTime date1 = DateTime.Parse("2020-01-05"), date2 = DateTime.Parse("2021-01-05");
        
        public NotesCrudServiceTests() {
            mockNotesRepo = new MockRepository<Note>();

            mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Repository<Note>()).Returns(mockNotesRepo.Object);

            notesCrudService = new NotesCrudService(mockUnitOfWork.Object);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesCrudService_GetById_ReturnsNote(bool performOwnershipValidation) {
            // Arrange
            var resultNote = new Note { Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);

            // Act
            ////// If there is a userId then we should perform an Ownership Validation check
            var result = await (performOwnershipValidation ? 
                notesCrudService.GetById(noteId, userId) : 
                notesCrudService.GetById(noteId)
            );

            // Assert
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            Assert.NotEqual(0, resultNote.Id);
            Assert.True(resultNote.Id > 0);
            Assert.True(result.CreationDate != null);
            Assert.True(result.ModificationDate != null);
        }

        [Fact]
        public async Task NotesCrudService_GetById_OwnershipValidation_ThrowsEntityDoesNotBelongToUser() {
            // Arrange
            var resultNote = new Note { Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);

            // Act
            Task act() => notesCrudService.GetById(noteId, 25);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EntityDoesNotBelongToUser.Detail.Message, exception.Detail.Message);
            Assert.Equal(403, ErrorStatusCode.EntityDoesNotBelongToUser.HttpStatusCode);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesCrudService_GetById_ThrowsEntityNotFound(bool performOwnershipValidation) {
            //Arrange
            mockNotesRepo.SetupGetByIdResult(null);

            // Act
            ////// If there is a userId then we should perform an Ownership Validation check
            Task act() => (performOwnershipValidation ?
                notesCrudService.GetById(noteId, userId) :
                notesCrudService.GetById(noteId)
            );

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EntityNotFound.Detail.Message, exception.Detail.Message);
            Assert.Equal(404, ErrorStatusCode.EntityNotFound.HttpStatusCode);
        }
        [Fact]
        public async Task NotesCrudService_GetUserNotes_ReturnsListOfNotes() {
            // Arrange
            var resultNotes = new List<Note> {
                new Note{ Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 }
            };
            mockNotesRepo.SetupGetMultipleResults(resultNotes);

            // Act
            var result = await notesCrudService.GetUserNotes(userId);

            // Assert
            mockNotesRepo.VerifyGetCalls(Times.Once());
            Assert.IsType<List<NoteDTO>>(result);
        }
        [Fact]
        public async Task NotesCrudService_GetUserNotes_ThrowsUserIdNotSupplied() {
            // Arrange

            // Act
            Task act() => notesCrudService.GetUserNotes(0);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.UserIdNotSupplied.Detail.Message, exception.Detail.Message);
            Assert.Equal(400, ErrorStatusCode.UserIdNotSupplied.HttpStatusCode);
        }
        [Fact]
        public async Task NotesCrudService_Post_ReturnsNote() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
            var resultNote = new Note { Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupPostResult(resultNote);

            // Act
            var result = await notesCrudService.Post(inputData);

            // Assert
            mockNotesRepo.VerifyPostCalls(Times.Once());
            Assert.NotEqual(0, resultNote.Id);
            Assert.True(resultNote.Id > 0);
            Assert.True(result.CreationDate != null);
            Assert.True(result.ModificationDate != null);
        }
        [Fact]
        public async Task NotesCrudService_Post_ThrowsUserIdNotSupplied() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = 0, Id = noteId };

            // Act
            Task act() => notesCrudService.Post(inputData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyPostCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.UserIdNotSupplied.Detail.Message, exception.Detail.Message);
            Assert.Equal(400, ErrorStatusCode.UserIdNotSupplied.HttpStatusCode);
        }
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task NotesCrudService_Post_ThrowsNoteTitleNotFound(string noteTitle) {
            // Arrange
            var inputData = new NoteDTO { Title = noteTitle, Content = content, UserId = userId };

            // Act
            Task act() => notesCrudService.Post(inputData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyPostCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.NoteTitleNotFound.Detail.Message, exception.Detail.Message);
            Assert.Equal(400, ErrorStatusCode.NoteTitleNotFound.HttpStatusCode);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesCrudService_Put_ReturnsModifiedNote(bool performOwnershipValidation) {
            // Arrange
            long noteUserId = performOwnershipValidation ? userId : 100;
            var inputData = new NoteDTO { Title = title, Content = content, UserId = noteUserId, Id = noteId };
            var resultNote = new Note { Title = title, Content = content, UserId = noteUserId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            mockNotesRepo.SetupPutResult(true);
            // Act
            ////// If there is a userId then we should perform an Ownership Validation check
            var result = await (performOwnershipValidation ?
                notesCrudService.Put(noteId, inputData, userId) :
                notesCrudService.Put(noteId, inputData)
            );
            // Assert
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyPutCalls(Times.Once());
            Assert.NotEqual(0, result.Id);
            Assert.True(result.Id > 0);
            Assert.True(result.CreationDate != null);
            Assert.True(result.ModificationDate != null);
        }

        [Fact]
        public async Task NotesCrudService_Put_ThrowsUserIdNotSupplied() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = 0, Id = noteId };
            // Act
            Task act() => notesCrudService.Put(noteId, inputData);
            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Never());
            mockNotesRepo.VerifyPutCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.UserIdNotSupplied.Detail.Message, exception.Detail.Message);
            Assert.Equal(400, ErrorStatusCode.UserIdNotSupplied.HttpStatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task NotesCrudService_Put_ThrowsNoteTitleNotFound(string noteTitle) {
            // Arrange
            var inputData = new NoteDTO { Title = noteTitle, Content = content, UserId = userId, Id = noteId };

            // Act
            Task act() => notesCrudService.Put(noteId, inputData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Never());
            mockNotesRepo.VerifyPutCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.NoteTitleNotFound.Detail.Message, exception.Detail.Message);
            Assert.Equal(400, ErrorStatusCode.NoteTitleNotFound.HttpStatusCode);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesCrudService_Put_ThrowsEntityNotFound(bool performOwnershipValidation) {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = userId, Id = noteId };
            mockNotesRepo.SetupGetByIdResult(null);
            mockNotesRepo.SetupPutResult(false);

            // Act
            Task act() => (performOwnershipValidation ?
                notesCrudService.Put(noteId, inputData, userId) :
                notesCrudService.Put(noteId, inputData)
            );

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyPutCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EntityNotFound.Detail.Message, exception.Detail.Message);
            Assert.Equal(404, ErrorStatusCode.EntityNotFound.HttpStatusCode);
        }
        [Fact]
        public async Task NotesCrudService_Put_ThrowsEntityDoesNotBelongToUser() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
            var resultNote = new Note { Title = title, Content = content, UserId = 25, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            mockNotesRepo.SetupPutResult(false);
            // Act
            Task act() => notesCrudService.Put(noteId, inputData, userId);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyPutCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EntityDoesNotBelongToUser.Detail.Message, exception.Detail.Message);
            Assert.Equal(403, ErrorStatusCode.EntityDoesNotBelongToUser.HttpStatusCode);
        }
        [Fact]
        public async Task NotesCrudService_Put_ThrowsRepoProblem() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
            var resultNote = new Note { Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            mockNotesRepo.SetupPutResult(false);
            // Act
            Task act() => notesCrudService.Put(noteId, inputData, userId);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyPutCalls(Times.Once());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.RepoProblem.Detail.Message, exception.Detail.Message);
            Assert.Equal(500, ErrorStatusCode.RepoProblem.HttpStatusCode);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesCrudService_Delete_ReturnsSuccessfulDeleteResult(bool performOwnershipValidation) {
            // Arrange
            long noteUserId = performOwnershipValidation ? userId : 100;
            var resultNote = new Note { Title = title, Content = content, UserId = noteUserId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            mockNotesRepo.SetupDeleteResult(true);
            // Act
            ////// If there is a userId then we should perform an Ownership Validation check
            var result = await (performOwnershipValidation ?
                notesCrudService.Delete(noteId, userId) :
                notesCrudService.Delete(noteId)
            );
            // Assert
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyDeleteCalls(Times.Once());
            Assert.True(result);
        }
        [Fact]
        public async Task NotesCrudService_Delete_ThrowsNoteIdNotSupplied() {
            // Arrange
            var inputData = 0;
            // Act
            Task act() => notesCrudService.Delete(inputData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Never());
            mockNotesRepo.VerifyDeleteCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.NoteIdNotSupplied.Detail.Message, exception.Detail.Message);
            Assert.Equal(400, ErrorStatusCode.NoteIdNotSupplied.HttpStatusCode);
        }
        [Fact]
        public async Task NotesCrudService_Delete_ThrowsEntityDoesNotBelongToUser() {
            // Arrange
            var resultNote = new Note { Title = title, Content = content, UserId = 25, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            mockNotesRepo.SetupDeleteResult(false);
            // Act
            Task act() => notesCrudService.Delete(noteId, userId);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyDeleteCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EntityDoesNotBelongToUser.Detail.Message, exception.Detail.Message);
            Assert.Equal(403, ErrorStatusCode.EntityDoesNotBelongToUser.HttpStatusCode);
        }
    }
}
