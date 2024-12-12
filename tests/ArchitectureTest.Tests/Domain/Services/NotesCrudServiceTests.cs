using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using ArchitectureTest.Tests.Shared.Mocks;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService;
using ArchitectureTest.Domain.Models.Enums;

namespace ArchitectureTest.Tests.Domain.Services;

public class NotesCrudServiceTests {
    private readonly MockRepository<Note> mockNotesRepo;
    private readonly Mock<IUnitOfWork> mockUnitOfWork;
    private readonly NotesCrudService notesCrudService;
    private const long userId = 1, noteId = 10;
    private const string title = "anyTitle", content = "anyContent";
    private const string randomToken = "eyzhdhhdhd.fhfhhf.fggg", randomRefreshToken = "4nyR3fr35hT0k3n";
    private DateTime date1 = DateTime.Now.Date, date2 = DateTime.Now.AddDays(5).Date;
    
    public NotesCrudServiceTests() {
        mockNotesRepo = new MockRepository<Note>();

        mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Repository<Note>()).Returns(mockNotesRepo.Object);

        notesCrudService = new NotesCrudService(mockUnitOfWork.Object);
        notesCrudService.CrudSettings = new EntityCrudSettings { UserId = userId };
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesCrudService_GetById_ReturnsNote(bool performOwnershipValidation) {
        // Arrange
        var resultNote = new Note {
            Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupGetByIdResult(resultNote);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation){
            notesCrudService.CrudSettings.UserId = userId;
			}
        // Act
        var result = await notesCrudService.GetById(noteId);

        // Assert
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        Assert.NotEqual(0, resultNote.Id);
        Assert.True(resultNote.Id > 0);
        Assert.True(result.CreationDate != default);
        Assert.True(result.ModificationDate != default);
    }

    [Fact]
    public async Task NotesCrudService_GetById_OwnershipValidation_ThrowsEntityDoesNotBelongToUser() {
        // Arrange
        var resultNote = new Note {
            Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupGetByIdResult(resultNote);

        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = 25; // 25 is not the owner of the resultNote
        // Act
        Task act() => notesCrudService.GetById(noteId);

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EntityDoesNotBelongToUser, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesCrudService_GetById_ThrowsEntityNotFound(bool performOwnershipValidation) {
        //Arrange
        mockNotesRepo.SetupGetByIdResult(null);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            notesCrudService.CrudSettings.UserId = userId;
        }
        // Act
        ////// If there is a userId then we should perform an Ownership Validation check
        Task act() => notesCrudService.GetById(noteId);

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EntityNotFound, exception.Message);
    }

    [Fact]
    public async Task NotesCrudService_GetUserNotes_ReturnsListOfNotes() {
        // Arrange
        var resultNotes = new List<Note> {
            new Note{
                Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2
            }
        };
        mockNotesRepo.SetupFindMultipleResults(resultNotes);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = userId;
        // Act
        var result = await notesCrudService.GetUserNotes();

        // Assert
        mockNotesRepo.VerifyFindCalls(Times.Once());
        Assert.IsType<List<NoteDTO>>(result);
    }

    [Fact]
    public async Task NotesCrudService_GetUserNotes_ThrowsUserIdNotSupplied() {
        // Arrange
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = 0;

        // Act
        Task act() => notesCrudService.GetUserNotes();

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyFindCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.UserIdNotSupplied, exception.Message);
    }

    [Fact]
    public async Task NotesCrudService_Post_ReturnsNote() {
        // Arrange
        var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
        var resultNote = new Note {
            Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupAddEntityResult(resultNote);

        // Act
        var result = await notesCrudService.Add(inputData);

        // Assert
        mockNotesRepo.VerifyAddEntityCalls(Times.Once());
        Assert.NotEqual(0, resultNote.Id);
        Assert.True(resultNote.Id > 0);
        Assert.True(result.CreationDate != default);
        Assert.True(result.ModificationDate != default);
    }

    [Fact]
    public async Task NotesCrudService_Post_ThrowsUserIdNotSupplied() {
        // Arrange
        var inputData = new NoteDTO { Title = title, Content = content, UserId = 0, Id = noteId };

        // Act
        Task act() => notesCrudService.Add(inputData);

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyAddEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.UserIdNotSupplied, exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task NotesCrudService_Post_ThrowsNoteTitleNotFound(string noteTitle) {
        // Arrange
        var inputData = new NoteDTO { Title = noteTitle, Content = content, UserId = userId };

        // Act
        Task act() => notesCrudService.Add(inputData);

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyAddEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.NoteTitleNotFound, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesCrudService_Put_ReturnsModifiedNote(bool performOwnershipValidation) {
        // Arrange
        long noteUserId = performOwnershipValidation ? userId : 100;
        var inputData = new NoteDTO {
            Title = title, Content = content, UserId = noteUserId, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        var resultNote = inputData.ToEntity();
        mockNotesRepo.SetupGetByIdResult(resultNote);
        mockNotesRepo.SetupUpdateEntityResult(true);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            notesCrudService.CrudSettings.UserId = userId;
        }
        // Act
        ////// If there is a userId then we should perform an Ownership Validation check
        var result = await notesCrudService.Update(noteId, inputData);
        // Assert
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Once());
        Assert.NotEqual(0, result.Id);
        Assert.True(result.Id > 0);
        Assert.True(result.CreationDate != default);
        Assert.True(result.ModificationDate != default);
    }

    [Fact]
    public async Task NotesCrudService_Put_ThrowsUserIdNotSupplied() {
        // Arrange
        var inputData = new NoteDTO { Title = title, Content = content, UserId = 0, Id = noteId };
        // Act
        Task act() => notesCrudService.Update(noteId, inputData);
        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyGetByIdCalls(Times.Never());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.UserIdNotSupplied, exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task NotesCrudService_Put_ThrowsNoteTitleNotFound(string noteTitle) {
        // Arrange
        var inputData = new NoteDTO { Title = noteTitle, Content = content, UserId = userId, Id = noteId };

        // Act
        Task act() => notesCrudService.Update(noteId, inputData);

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyGetByIdCalls(Times.Never());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.NoteTitleNotFound, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesCrudService_Put_ThrowsEntityNotFound(bool performOwnershipValidation) {
        // Arrange
        var inputData = new NoteDTO { Title = title, Content = content, UserId = userId, Id = noteId };
        mockNotesRepo.SetupGetByIdResult(null);
        mockNotesRepo.SetupUpdateEntityResult(false);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            notesCrudService.CrudSettings.UserId = userId;
        }
        // Act
        Task act() => notesCrudService.Update(noteId, inputData);

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EntityNotFound, exception.Message);
    }

    [Fact]
    public async Task NotesCrudService_Put_ThrowsEntityDoesNotBelongToUser() {
        // Arrange
        var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
        var resultNote = new Note {
            Title = title, Content = content, UserId = 25, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupGetByIdResult(resultNote);
        mockNotesRepo.SetupUpdateEntityResult(false);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = userId;
        // Act
        Task act() => notesCrudService.Update(noteId, inputData);

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EntityDoesNotBelongToUser, exception.Message);
    }

    [Fact]
    public async Task NotesCrudService_Put_ThrowsRepoProblem() {
        // Arrange
        var inputData = new NoteDTO { Title = title, Content = content, UserId = userId };
        var resultNote = new Note {
            Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupGetByIdResult(resultNote);
        mockNotesRepo.SetupUpdateEntityResult(false);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = userId;

        // Act
        Task act() => notesCrudService.Update(noteId, inputData);

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Once());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.RepoProblem, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesCrudService_Delete_ReturnsSuccessfulDeleteResult(bool performOwnershipValidation) {
        // Arrange
        long noteUserId = performOwnershipValidation ? userId : 100;
        var resultNote = new Note {
            Title = title, Content = content, UserId = noteUserId, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupGetByIdResult(resultNote);
        mockNotesRepo.SetupDeleteResult(true);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            notesCrudService.CrudSettings.UserId = userId;
        }
        // Act
        ////// If there is a userId then we should perform an Ownership Validation check
        var result = await notesCrudService.Delete(noteId);
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
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyGetByIdCalls(Times.Never());
        mockNotesRepo.VerifyDeleteCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.NoteIdNotSupplied, exception.Message);
    }

    [Fact]
    public async Task NotesCrudService_Delete_ThrowsEntityDoesNotBelongToUser() {
        // Arrange
        var resultNote = new Note {
            Title = title, Content = content, UserId = 25, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupGetByIdResult(resultNote);
        mockNotesRepo.SetupDeleteResult(false);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = userId;
        // Act
        Task act() => notesCrudService.Delete(noteId);

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        mockNotesRepo.VerifyDeleteCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EntityDoesNotBelongToUser, exception.Message);
    }
}
