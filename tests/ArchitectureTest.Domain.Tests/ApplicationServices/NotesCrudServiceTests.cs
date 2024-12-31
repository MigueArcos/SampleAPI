using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;
using ArchitectureTest.TestUtils;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchitectureTest.Domain.Tests.ApplicationServices;

public class NotesCrudServiceTests {
    private readonly IRepository<Note> _mockNotesRepo;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly NotesCrudService _systemUnderTest;

    public NotesCrudServiceTests() {
        _mockNotesRepo = Substitute.For<IRepository<Note>>();

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();

        _mockUnitOfWork.Repository<Note>().Returns(_mockNotesRepo);

        _systemUnderTest = new NotesCrudService(_mockUnitOfWork) {
            CrudSettings = new EntityCrudSettings {
                UserId = StubData.UserId
            }
        };
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetById_WhenEverythingIsOK_ReturnsNote(bool performOwnershipValidation)
    {
        // Arrange
        var getByIdNote = BuildNote();
        _mockNotesRepo.GetById(getByIdNote.Id).Returns(getByIdNote);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        var result = await _systemUnderTest.GetById(getByIdNote.Id);

        // Assert
        await _mockNotesRepo.Received(1).GetById(getByIdNote.Id);
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        ObjectComparer.JsonCompare(getByIdNote, result.Value).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WhenOwnershipValidationFails_ReturnsError()
    {
        // Arrange
        var getByIdNote = BuildNote();
        _mockNotesRepo.GetById(getByIdNote.Id).Returns(getByIdNote);

        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = "25"; // 25 is not the owner of the resultNote

        // Act
        var result = await _systemUnderTest.GetById(getByIdNote.Id);

        // Assert
        await _mockNotesRepo.Received(1).GetById(getByIdNote.Id);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.EntityDoesNotBelongToUser);
    }

    [Fact]
    public async Task GetById_WhenNoValidationsFoundForOperation_ReturnsError()
    {
        // Arrange
        _systemUnderTest.ValidationsByOperation.Remove(Enums.CrudOperation.ReadById);

        // Act
        var result = await _systemUnderTest.GetById(StubData.NoteId);

        // Assert
        await _mockNotesRepo.DidNotReceive().GetById(StubData.NoteId);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.IncorrectInputData);
    }

    [Fact]
    public async Task GetById_WhenNoteIdNotProvided_ReturnsError()
    {
        // Arrange
        string inputId = string.Empty;

        // Act
        var result = await _systemUnderTest.GetById(inputId);

        // Assert
        await _mockNotesRepo.DidNotReceiveWithAnyArgs().GetById(default!);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.NoteIdNotSupplied);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetById_WhenNoteNotFound_ReturnsError(bool performOwnershipValidation)
    {
        // Arrange
        _mockNotesRepo.GetById(StubData.NoteId).Returns((Note) null!);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }
        // Act
        ////// If there is a userId then we should perform an Ownership Validation check
        var result = await _systemUnderTest.GetById(StubData.NoteId);

        // Assert
        await _mockNotesRepo.Received(1).GetById(StubData.NoteId);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.EntityNotFound);
    }

    [Fact]
    public async Task GetUserNotes_WhenEverythingIsOK_ReturnsListOfNotes()
    {
        // Arrange
        var foundNotes = new List<Note> {
            BuildNote(noteId: "1"),
            BuildNote(noteId: "2"),
            BuildNote(noteId: "3")
        };
        _mockNotesRepo.Find(default).ReturnsForAnyArgs(foundNotes);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = StubData.UserId;

        // Act
        var result = await _systemUnderTest.GetUserNotes();

        // Assert
        await _mockNotesRepo.ReceivedWithAnyArgs(1).Find(default);
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        ObjectComparer.JsonCompare(foundNotes, result.Value).Should().BeTrue();
    }

    [Fact]
    public async Task GetUserNotes_WhenUserIdNotProvided_ReturnsError()
    {
        // Arrange
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = string.Empty;

        // Act
        var result = await _systemUnderTest.GetUserNotes();

        // Assert
        await _mockNotesRepo.DidNotReceiveWithAnyArgs().GetById(StubData.NoteId);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.UserIdNotSupplied);
    }

    [Fact]
    public async Task Create_WhenEverythingIsOK_ReturnsNote()
    {
        // Arrange
        var today = StubData.Today;
        var nextWeek = StubData.NextWeek;
        var inputData = BuildNote(noteId: string.Empty, creationDate: today, modificationDate: nextWeek);
        var resultNote = BuildNote(creationDate: today, modificationDate: nextWeek);
        _mockNotesRepo.Create(inputData, true).Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.Create(inputData);

        // Assert
        await _mockNotesRepo.Received(1).Create(inputData);
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        ObjectComparer.JsonCompare(inputData, result.Value, [nameof(Note.Id)]).Should().BeTrue();
    }

    [Fact]
    public async Task Create_WhenUserIdNotProvided_ReturnsError()
    {
        // Arrange
        var inputData = BuildNote(userId: string.Empty);

        // Act
        var result = await _systemUnderTest.Create(inputData);

        // Assert
        await _mockNotesRepo.DidNotReceive().Create(default!, default);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.UserIdNotSupplied);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Create_WhenNoteTitleNotProvided_ReturnsError(string? noteTitle)
    {
        // Arrange
        var inputData = BuildNote(title: noteTitle!);

        // Act
        var result = await _systemUnderTest.Create(inputData);

        // Assert
        await _mockNotesRepo.DidNotReceive().Create(default!, default);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.NoteTitleNotFound);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_WhenEverythingIsOK_ReturnsModifiedNote(bool performOwnershipValidation)
    {
        // Arrange
        string noteUserId = performOwnershipValidation ? StubData.UserId : "100";
        var getByIdNote = BuildNote(userId: noteUserId);
        // var resultNote = inputData.ToEntity();
        _mockNotesRepo.GetById(StubData.NoteId).Returns(getByIdNote);
        _mockNotesRepo.Update(getByIdNote, true).Returns(Task.CompletedTask);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        ////// If there is a userId then we should perform an Ownership Validation check
        var result = await _systemUnderTest.Update(StubData.NoteId, getByIdNote);

        // Assert
        await _mockNotesRepo.Received(1).GetById(StubData.NoteId);
        await _mockNotesRepo.Received(1).Update(getByIdNote, true);
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.ModificationDate.Should().NotBeNull();
        ObjectComparer.JsonCompare(getByIdNote, result.Value).Should().BeTrue();
    }

    [Fact]
    public async Task Update_WhenUserIdNotProvided_ReturnsError()
    {
        // Arrange
        var inputData = BuildNote(userId: string.Empty);

        // Act
        var result = await _systemUnderTest.Update(StubData.NoteId, inputData);

        // Assert
        await _mockNotesRepo.DidNotReceive().GetById(StubData.NoteId);
        await _mockNotesRepo.DidNotReceive().Update(default!, default);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.UserIdNotSupplied);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Update_WhenNoteTitleNotProvided_ReturnsError(string? noteTitle)
    {
        // Arrange
        var inputData = BuildNote(title: noteTitle!);

        // Act
        var result = await _systemUnderTest.Update(StubData.NoteId, inputData);

        // Assert
        await _mockNotesRepo.DidNotReceive().GetById(StubData.NoteId);
        await _mockNotesRepo.DidNotReceive().Update(default!, default);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.NoteTitleNotFound);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_WhenNoteNotFound_ReturnsError(bool performOwnershipValidation)
    {
        // Arrange
        var inputData = BuildNote(noteId: string.Empty);
        _mockNotesRepo.GetById(StubData.NoteId).Returns((Note) default!);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        var result = await _systemUnderTest.Update(StubData.NoteId, inputData);

        // Assert
        await _mockNotesRepo.Received(1).GetById(StubData.NoteId);
        await _mockNotesRepo.DidNotReceive().Update(default!, default);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.EntityNotFound);
    }

    [Fact]
    public async Task Update_WhenNoteDoesNotBelongToUser_ReturnsError()
    {
        // Arrange
        var inputData = BuildNote(noteId: string.Empty);
        var resultNote = BuildNote(userId: "25");
        _mockNotesRepo.GetById(StubData.NoteId).Returns(resultNote);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = StubData.UserId;

        // Act
        var result = await _systemUnderTest.Update(StubData.NoteId, inputData);

        // Assert
        await _mockNotesRepo.Received(1).GetById(StubData.NoteId);
        await _mockNotesRepo.DidNotReceive().Update(default!, default);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.EntityDoesNotBelongToUser);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteById_WhenEverythingIsOK_ReturnsSuccess(bool performOwnershipValidation)
    {
        // Arrange
        string noteUserId = performOwnershipValidation ? StubData.UserId : "100";
        var resultNote = BuildNote(userId: noteUserId);
        _mockNotesRepo.GetById(StubData.NoteId).Returns(resultNote);
        _mockNotesRepo.DeleteById(StubData.NoteId, true).Returns(Task.CompletedTask);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        ////// If there is a userId then we should perform an Ownership Validation check
        var result = await _systemUnderTest.DeleteById(StubData.NoteId);

        // Assert
        await _mockNotesRepo.Received(1).GetById(StubData.NoteId);
        await _mockNotesRepo.Received(1).DeleteById(StubData.NoteId, true);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteById_WhenNoteIdNotProvided_ReturnsError()
    {
        // Arrange
        string inputId = string.Empty;

        // Act
        var result = await _systemUnderTest.DeleteById(inputId);

        // Assert
        await _mockNotesRepo.DidNotReceive().GetById(StubData.NoteId);
        await _mockNotesRepo.DidNotReceive().DeleteById(default!, default);
        result.Should().NotBeNull();
        result!.Code.Should().Be(ErrorCodes.NoteIdNotSupplied);
    }

    [Fact]
    public async Task DeleteById_WhenNoteNotFound_ReturnsError()
    {
        // Arrange
        _mockNotesRepo.GetById(StubData.NoteId).Returns((Note) default!);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = StubData.UserId;

        // Act
        var result = await _systemUnderTest.DeleteById(StubData.NoteId);

        // Assert
        await _mockNotesRepo.Received(1).GetById(StubData.NoteId);
        await _mockNotesRepo.DidNotReceive().DeleteById(default!, default);
        result.Should().NotBeNull();
        result!.Code.Should().Be(ErrorCodes.EntityNotFound);
    }

    [Fact]
    public async Task Delete_WhenNoteDoesNotBelongToUser_ReturnsError()
    {
        // Arrange
        var getByIdNote = BuildNote(userId: "25");
        _mockNotesRepo.GetById(StubData.NoteId).Returns(getByIdNote);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = StubData.UserId;

        // Act
        var result = await _systemUnderTest.DeleteById(StubData.NoteId);

        // Assert
        await _mockNotesRepo.Received(1).GetById(StubData.NoteId);
        await _mockNotesRepo.DidNotReceive().DeleteById(default!, default);
        result.Should().NotBeNull();
        result!.Code.Should().Be(ErrorCodes.EntityDoesNotBelongToUser);
    }

    private Note BuildNote(
        string noteId = StubData.NoteId, string title = StubData.NoteTitle, string content = StubData.NoteContent,
        string userId = StubData.UserId, DateTime? creationDate = null, DateTime? modificationDate = null
    ) {
        return new Note {
            Id = noteId,
            Title = title,
            Content = content,
            UserId = userId,
            CreationDate = creationDate ?? StubData.Today,
            ModificationDate = modificationDate ?? StubData.NextWeek
        };
    }
}
