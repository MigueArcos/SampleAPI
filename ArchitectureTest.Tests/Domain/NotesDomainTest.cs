using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Domain.UnitOfWork;
using ArchitectureTest.Tests.Shared.Mocks;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchitectureTest.Tests.Domain {
    public class NotesDomainTest {
        private readonly MockRepository<Note> mockNotesRepo;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;

        private const long userId = 1, noteId = 10;
        private const string title = "anyTitle", content = "anyContent";
        private const string randomToken = "eyzhdhhdhd.fhfhhf.fggg", randomRefreshToken = "4nyR3fr35hT0k3n";
        private DateTime date1 = DateTime.Parse("2020-01-05"), date2 = DateTime.Parse("2021-01-05");
        
        public NotesDomainTest() {
            mockNotesRepo = new MockRepository<Note>();

            mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Repository<Note>()).Returns(mockNotesRepo.Object);
        }
        [Theory]
        [InlineData(noteId, userId, userId, title, content)]
        [InlineData(noteId, userId, null, title, content)]
        public async Task NotesDomain_GetById_NoOwnershipValidation_ReturnsNote(long noteId, long noteUserId, long? userId, string title, string content) {
            // Arrange
            var resultNote = new Note { Title = title, Content = content, UserId = noteUserId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            var notesDomain = new NotesDomain(mockUnitOfWork.Object);

            // Act
            ////// If there is a userId then we should perform an Ownership Validation check
            var result = await (userId.HasValue ? 
                notesDomain.GetById(noteId, userId.Value) : 
                notesDomain.GetById(noteId)
            );

            // Assert
            mockNotesRepo.VerifyGetByIdCalls(Times.Once());
            // Assert.NotEqual(0, resultNote.Id);
            Assert.True(resultNote.Id > 0);
            Assert.True(resultNote.CreationDate != null);
            Assert.True(resultNote.ModificationDate != null);
        }

        [Fact]
        public async Task NotesDomain_GetById_OwnershipValidation_ThrowsEntityDoesNotBelongToUser() {
            // Arrange
            var resultNote = new Note { Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupGetByIdResult(resultNote);
            var notesDomain = new NotesDomain(mockUnitOfWork.Object);

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
        [InlineData(noteId, userId)]
        [InlineData(noteId, null)]
        public async Task NotesDomain_GetById_ThrowsEntityNotFound(long noteId, long? userId) {
            //Arrange
            mockNotesRepo.SetupGetByIdResult(null);
            var notesDomain = new NotesDomain(mockUnitOfWork.Object);

            // Act
            ////// If there is a userId then we should perform an Ownership Validation check
            Task act() => (userId.HasValue ?
                notesDomain.GetById(noteId, userId.Value) :
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
        public async Task NotesDomain_Post_ReturnsNote() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
            var resultNote = new Note { Title = title, Content = content, UserId = userId, Id = 1000, CreationDate = date1, ModificationDate = date2 };
            mockNotesRepo.SetupPostResult(resultNote);
            var notesDomain = new NotesDomain(mockUnitOfWork.Object);

            // Act
            var result = await notesDomain.Post(inputData);

            // Assert
            mockNotesRepo.VerifyPostCalls(Times.Once());
            // Assert.NotEqual(0, resultNote.Id);
            Assert.True(resultNote.Id > 0);
            Assert.True(resultNote.CreationDate != null);
            Assert.True(resultNote.ModificationDate != null);
        }
        [Fact]
        public async Task NotesDomain_Post_ThrowsUserIdNotSupplied() {
            // Arrange
            var inputData = new NoteDTO { Title = title, Content = content, UserId = 0 };
            var notesDomain = new NotesDomain(mockUnitOfWork.Object);

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
        [InlineData(1, "", content)]
        [InlineData(1, null, content)]
        [InlineData(1, "   ", content)]
        public async Task NotesDomain_Post_ThrowsNoteTitleNotFound(long userId, string noteTitle, string noteContent) {
            // Arrange
            var inputData = new NoteDTO { Title = noteTitle, Content = noteContent, UserId = userId };
            var notesDomain = new NotesDomain(mockUnitOfWork.Object);

            // Act
            Task act() => notesDomain.Post(inputData);

            // Assert
            ErrorStatusCode exception = await Assert.ThrowsAsync<ErrorStatusCode>(act);
            mockNotesRepo.VerifyPostCalls(Times.Never());
            //The thrown exception can be used for even more detailed assertions.
            Assert.Equal(ErrorStatusCode.NoteTitleNotFound.StatusCode.Message, exception.StatusCode.Message);
            Assert.Equal(400, ErrorStatusCode.NoteTitleNotFound.HttpStatusCode);
        }
    }
}
