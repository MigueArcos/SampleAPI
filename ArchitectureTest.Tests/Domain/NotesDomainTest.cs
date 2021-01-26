using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Domain.UnitOfWork;
using ArchitectureTest.Tests.Shared.Mocks;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchitectureTest.Tests.Domain {
    public class NotesDomainTest {
        private readonly MockRepository<Note> mockNotesRepo;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly NotesDomain notesDomain;
        private const long userId = 1, noteId = 10;
        private const string title = "anyTitle", content = "anyContent";
        private const string randomToken = "eyzhdhhdhd.fhfhhf.fggg", randomRefreshToken = "4nyR3fr35hT0k3n";
        private DateTime date1 = DateTime.Parse("2020-01-05"), date2 = DateTime.Parse("2021-01-05");
        
        public NotesDomainTest() {
            mockNotesRepo = new MockRepository<Note>();

            mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Repository<Note>()).Returns(mockNotesRepo.Object);

            notesDomain = new NotesDomain(mockUnitOfWork.Object);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesDomain_GetById_ReturnsNote(bool performOwnershipValidation) {
            // Arrange
            var resultNote = new Note { Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);

            // Act
            ////// If there is a userId then we should perform an Ownership Validation check
            var result = await (performOwnershipValidation ? 
                notesDomain.GetById(noteId, userId) : 
                notesDomain.GetById(noteId)
            );

            // Assert
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            Assert.NotEqual(0, resultNote.Id);
            Assert.True(resultNote.Id > 0);
            Assert.True(result.CreationDate != null);
            Assert.True(result.ModificationDate != null);
        }

        [Fact]
        public async Task NotesDomain_GetById_OwnershipValidation_ThrowsEntityDoesNotBelongToUser() {
            // Arrange
            var resultNote = new Note { Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);

            // Act
            Task act() => notesDomain.GetById(noteId, 25);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EntityDoesNotBelongToUser.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(403, ErrorStatusCode.EntityDoesNotBelongToUser.HttpStatusCode);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesDomain_GetById_ThrowsEntityNotFound(bool performOwnershipValidation) {
            //Arrange
            mockNotesRepo.SetupGetByIdResult(null);

            // Act
            ////// If there is a userId then we should perform an Ownership Validation check
            Task act() => (performOwnershipValidation ?
                notesDomain.GetById(noteId, userId) :
                notesDomain.GetById(noteId)
            );

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EntityNotFound.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(404, ErrorStatusCode.EntityNotFound.HttpStatusCode);
        }
        [Fact]
        public async Task NotesDomain_GetUserNotes_ReturnsListOfNotes() {
            // Arrange
            var resultNotes = new List<Note> {
                new Note{ Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 }
            };
            mockNotesRepo.SetupGetMultipleResults(resultNotes);

            // Act
            var result = await notesDomain.GetUserNotes(userId);

            // Assert
            mockNotesRepo.VerifyGetCalls(Times.Once());
            Assert.IsType<List<NoteDTO>>(result);
        }
        [Fact]
        public async Task NotesDomain_GetUserNotes_ThrowsUserIdNotSupplied() {
            // Arrange

            // Act
            Task act() => notesDomain.GetUserNotes(0);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.UserIdNotSupplied.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(400, ErrorStatusCode.UserIdNotSupplied.HttpStatusCode);
        }
        [Fact]
        public async Task NotesDomain_Post_ReturnsNote() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
            var resultNote = new Note { Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupPostResult(resultNote);

            // Act
            var result = await notesDomain.Post(inputData);

            // Assert
            mockNotesRepo.VerifyPostCalls(Times.Once());
            Assert.NotEqual(0, resultNote.Id);
            Assert.True(resultNote.Id > 0);
            Assert.True(result.CreationDate != null);
            Assert.True(result.ModificationDate != null);
        }
        [Fact]
        public async Task NotesDomain_Post_ThrowsUserIdNotSupplied() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = 0, Id = noteId };

            // Act
            Task act() => notesDomain.Post(inputData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyPostCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.UserIdNotSupplied.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(400, ErrorStatusCode.UserIdNotSupplied.HttpStatusCode);
        }
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task NotesDomain_Post_ThrowsNoteTitleNotFound(string noteTitle) {
            // Arrange
            var inputData = new NoteDTO { Title = noteTitle, Content = content, UserId = userId };

            // Act
            Task act() => notesDomain.Post(inputData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyPostCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.NoteTitleNotFound.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(400, ErrorStatusCode.NoteTitleNotFound.HttpStatusCode);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesDomain_Put_ReturnsModifiedNote(bool performOwnershipValidation) {
            // Arrange
            long noteUserId = performOwnershipValidation ? userId : 100;
            var inputData = new NoteDTO { Title = title, Content = content, UserId = noteUserId, Id = noteId };
            var resultNote = new Note { Title = title, Content = content, UserId = noteUserId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            mockNotesRepo.SetupPutResult(true);
            // Act
            ////// If there is a userId then we should perform an Ownership Validation check
            var result = await (performOwnershipValidation ?
                notesDomain.Put(noteId, inputData, userId) :
                notesDomain.Put(noteId, inputData)
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
        public async Task NotesDomain_Put_ThrowsUserIdNotSupplied() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = 0, Id = noteId };
            // Act
            Task act() => notesDomain.Put(noteId, inputData);
            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Never());
            mockNotesRepo.VerifyPutCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.UserIdNotSupplied.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(400, ErrorStatusCode.UserIdNotSupplied.HttpStatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task NotesDomain_Put_ThrowsNoteTitleNotFound(string noteTitle) {
            // Arrange
            var inputData = new NoteDTO { Title = noteTitle, Content = content, UserId = userId, Id = noteId };

            // Act
            Task act() => notesDomain.Put(noteId, inputData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Never());
            mockNotesRepo.VerifyPutCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.NoteTitleNotFound.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(400, ErrorStatusCode.NoteTitleNotFound.HttpStatusCode);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesDomain_Put_ThrowsEntityNotFound(bool performOwnershipValidation) {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = userId, Id = noteId };
            mockNotesRepo.SetupGetByIdResult(null);
            mockNotesRepo.SetupPutResult(false);

            // Act
            Task act() => (performOwnershipValidation ?
                notesDomain.Put(noteId, inputData, userId) :
                notesDomain.Put(noteId, inputData)
            );

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyPutCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EntityNotFound.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(404, ErrorStatusCode.EntityNotFound.HttpStatusCode);
        }
        [Fact]
        public async Task NotesDomain_Put_ThrowsEntityDoesNotBelongToUser() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
            var resultNote = new Note { Title = title, Content = content, UserId = 25, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            mockNotesRepo.SetupPutResult(false);
            // Act
            Task act() => notesDomain.Put(noteId, inputData, userId);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyPutCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EntityDoesNotBelongToUser.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(403, ErrorStatusCode.EntityDoesNotBelongToUser.HttpStatusCode);
        }
        [Fact]
        public async Task NotesDomain_Put_ThrowsRepoProblem() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
            var resultNote = new Note { Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            mockNotesRepo.SetupPutResult(false);
            // Act
            Task act() => notesDomain.Put(noteId, inputData, userId);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyPutCalls(Times.Once());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.RepoProblem.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(500, ErrorStatusCode.RepoProblem.HttpStatusCode);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotesDomain_Delete_ReturnsSuccessfulDeleteResult(bool performOwnershipValidation) {
            // Arrange
            long noteUserId = performOwnershipValidation ? userId : 100;
            var resultNote = new Note { Title = title, Content = content, UserId = noteUserId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            mockNotesRepo.SetupDeleteResult(true);
            // Act
            ////// If there is a userId then we should perform an Ownership Validation check
            var result = await (performOwnershipValidation ?
                notesDomain.Delete(noteId, userId) :
                notesDomain.Delete(noteId)
            );
            // Assert
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyDeleteCalls(Times.Once());
            Assert.True(result);
        }
        [Fact]
        public async Task NotesDomain_Delete_ThrowsNoteIdNotSupplied() {
            // Arrange
            var inputData = 0;
            // Act
            Task act() => notesDomain.Delete(inputData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Never());
            mockNotesRepo.VerifyDeleteCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.NoteIdNotSupplied.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(400, ErrorStatusCode.NoteIdNotSupplied.HttpStatusCode);
        }
        [Fact]
        public async Task NotesDomain_Delete_ThrowsEntityDoesNotBelongToUser() {
            // Arrange
            var resultNote = new Note { Title = title, Content = content, UserId = 25, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            mockNotesRepo.SetupDeleteResult(false);
            // Act
            Task act() => notesDomain.Delete(noteId, userId);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            mockNotesRepo.VerifyDeleteCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.EntityDoesNotBelongToUser.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(403, ErrorStatusCode.EntityDoesNotBelongToUser.HttpStatusCode);
        }
    }
}
