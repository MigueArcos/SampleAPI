using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;
using ArchitectureTest.Tests.Shared.Mocks;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

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
        Assert.True(result.Value.CreationDate != default);
        Assert.True(result.Value.ModificationDate != default);
    }

    [Fact]
    public async Task NotesCrudService_GetById_OwnershipValidation_ReturnsEntityDoesNotBelongToUserError() {
        // Arrange
        var resultNote = new Note {
            Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupGetByIdResult(resultNote);

        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = 25; // 25 is not the owner of the resultNote

        // Act
        var result = await notesCrudService.GetById(noteId);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EntityDoesNotBelongToUser, result.Error.Code);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesCrudService_GetById_ReturnsEntityNotFoundError(bool performOwnershipValidation) {
        //Arrange
        mockNotesRepo.SetupGetByIdResult(null);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            notesCrudService.CrudSettings.UserId = userId;
        }
        // Act
        ////// If there is a userId then we should perform an Ownership Validation check
        var result = await notesCrudService.GetById(noteId);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EntityNotFound, result.Error.Code);
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
        Assert.NotNull(result);
        Assert.Null(result.Error);
        Assert.NotNull(result.Value);
        mockNotesRepo.VerifyFindCalls(Times.Once());
        Assert.IsType<List<Note>>(result.Value);
    }

    [Fact]
    public async Task NotesCrudService_GetUserNotes_ReturnsUserIdNotSuppliedError() {
        // Arrange
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = 0;

        // Act
        var result = await notesCrudService.GetUserNotes();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockNotesRepo.VerifyFindCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.UserIdNotSupplied, result.Error.Code);
    }

    [Fact]
    public async Task NotesCrudService_Add_ReturnsNote() {
        // Arrange
        var inputData = new Note { Title = title, Content = content, UserId = userId };
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
        Assert.True(result.Value.CreationDate != default);
        Assert.True(result.Value.ModificationDate != default);
    }

    [Fact]
    public async Task NotesCrudService_Add_ReturnsUserIdNotSuppliedError() {
        // Arrange
        var inputData = new Note { Title = title, Content = content, UserId = 0, Id = noteId };

        // Act
        var result = await notesCrudService.Add(inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockNotesRepo.VerifyAddEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.UserIdNotSupplied, result.Error.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task NotesCrudService_Add_ReturnsNoteTitleNotFoundError(string noteTitle) {
        // Arrange
        var inputData = new Note { Title = noteTitle, Content = content, UserId = userId };

        // Act
        var result = await notesCrudService.Add(inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockNotesRepo.VerifyAddEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.NoteTitleNotFound, result.Error.Code);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesCrudService_Update_ReturnsModifiedNote(bool performOwnershipValidation) {
        // Arrange
        long noteUserId = performOwnershipValidation ? userId : 100;
        var inputData = new Note {
            Title = title, Content = content, UserId = noteUserId, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        // var resultNote = inputData.ToEntity();
        mockNotesRepo.SetupGetByIdResult(inputData);
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
        Assert.NotEqual(0, result.Value.Id);
        Assert.True(result.Value.Id > 0);
        Assert.True(result.Value.CreationDate != default);
        Assert.True(result.Value.ModificationDate != default);
    }

    [Fact]
    public async Task NotesCrudService_Update_ReturnsUserIdNotSuppliedError() {
        // Arrange
        var inputData = new Note { Title = title, Content = content, UserId = 0, Id = noteId };

        // Act
        var result = await notesCrudService.Update(noteId, inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockNotesRepo.VerifyGetByIdCalls(Times.Never());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.UserIdNotSupplied, result.Error.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task NotesCrudService_Update_ReturnsNoteTitleNotFoundError(string noteTitle) {
        // Arrange
        var inputData = new Note { Title = noteTitle, Content = content, UserId = userId, Id = noteId };

        // Act
        var result = await notesCrudService.Update(noteId, inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockNotesRepo.VerifyGetByIdCalls(Times.Never());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.NoteTitleNotFound, result.Error.Code);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NotesCrudService_Update_ReturnsEntityNotFoundError(bool performOwnershipValidation) {
        // Arrange
        var inputData = new Note { Title = title, Content = content, UserId = userId, Id = noteId };
        mockNotesRepo.SetupGetByIdResult(null);
        mockNotesRepo.SetupUpdateEntityResult(false);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            notesCrudService.CrudSettings.UserId = userId;
        }
        // Act
        var result = await notesCrudService.Update(noteId, inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EntityNotFound, result.Error.Code);
    }

    [Fact]
    public async Task NotesCrudService_Update_ReturnsEntityDoesNotBelongToUserError() {
        // Arrange
        var inputData = new Note { Title = title, Content = content, UserId = userId };
        var resultNote = new Note {
            Title = title, Content = content, UserId = 25, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupGetByIdResult(resultNote);
        mockNotesRepo.SetupUpdateEntityResult(false);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = userId;

        // Act
        var result = await notesCrudService.Update(noteId, inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EntityDoesNotBelongToUser, result.Error.Code);
    }

    [Fact]
    public async Task NotesCrudService_Update_ReturnsRepoProblemError() {
        // Arrange
        var inputData = new Note { Title = title, Content = content, UserId = userId };
        var resultNote = new Note {
            Title = title, Content = content, UserId = userId, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupGetByIdResult(resultNote);
        mockNotesRepo.SetupUpdateEntityResult(false);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = userId;

        // Act
        var result = await notesCrudService.Update(noteId, inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        mockNotesRepo.VerifyUpdateEntityCalls(Times.Once());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.RepoProblem, result.Error.Code);
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
        Assert.Null(result);
    }

    [Fact]
    public async Task NotesCrudService_Delete_ReturnsNoteIdNotSuppliedError() {
        // Arrange
        var inputData = 0;
        // Act
        var result = await notesCrudService.Delete(inputData);

        // Assert
        Assert.NotNull(result);
        mockNotesRepo.VerifyGetByIdCalls(Times.Never());
        mockNotesRepo.VerifyDeleteCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.NoteIdNotSupplied, result.Code);
    }

    [Fact]
    public async Task NotesCrudService_Delete_ReturnsEntityDoesNotBelongToUserError() {
        // Arrange
        var resultNote = new Note {
            Title = title, Content = content, UserId = 25, Id = noteId, CreationDate = date1, ModificationDate = date2
        };
        mockNotesRepo.SetupGetByIdResult(resultNote);
        mockNotesRepo.SetupDeleteResult(false);
        notesCrudService.CrudSettings.ValidateEntityBelongsToUser = true;
        notesCrudService.CrudSettings.UserId = userId;

        // Act
        var result = await notesCrudService.Delete(noteId);

        // Assert
        Assert.NotNull(result);
        mockNotesRepo.VerifyGetByIdCalls(Times.Once());
        mockNotesRepo.VerifyDeleteCalls(Times.Never());
        //The thrown exception can be used for even more detailed assertions.
        Assert.Equal(ErrorCodes.EntityDoesNotBelongToUser, result.Code);
    }
}
