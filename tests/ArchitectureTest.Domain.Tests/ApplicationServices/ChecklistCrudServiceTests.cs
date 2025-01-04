using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;
using ArchitectureTest.TestUtils;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchitectureTest.Domain.Tests.ApplicationServices;

public class ChecklistCrudServiceTests {
    private readonly IChecklistRepository _mockChecklistRepo;
    private readonly IRepository<ChecklistDetail> _mockChecklistDetailsRepo;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ChecklistCrudService _systemUnderTest;
    private readonly IMapper _mapper;
    private readonly ILogger<ChecklistCrudService> _mockLogger;

    public ChecklistCrudServiceTests() {
        _mockChecklistRepo = Substitute.For<IChecklistRepository>();
        _mockChecklistDetailsRepo = Substitute.For<IRepository<ChecklistDetail>>();

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();

        _mockLogger = Substitute.For<ILogger<ChecklistCrudService>>();

        _mockUnitOfWork.Repository<Checklist>().Returns(_mockChecklistRepo);
        _mockUnitOfWork.Repository<ChecklistDetail>().Returns(_mockChecklistDetailsRepo);

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<ApplicationModelsMappingProfile>());
        // mapperConfig.AssertConfigurationIsValid();
        _mapper = mapperConfig.CreateMapper();
        
        _systemUnderTest = new ChecklistCrudService(_mockUnitOfWork, _mapper, _mockLogger) {
            CrudSettings = new EntityCrudSettings {
                UserId = StubData.UserId
            }
        };
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetById_WhenEverythingIsOK_ReturnsChecklist(bool performOwnershipValidation)
    {
        // Arrange
        var getByIdChecklist = BuildChecklist();
        _mockChecklistRepo.GetById(getByIdChecklist.Id).Returns(getByIdChecklist);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        var result = await _systemUnderTest.GetById(getByIdChecklist.Id);

        // Assert
        await _mockChecklistRepo.Received(1).GetById(getByIdChecklist.Id);
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        var mappedResult = _mapper.Map<Checklist>(result.Value);
        ObjectComparer.JsonCompare(getByIdChecklist, mappedResult).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WhenOwnershipValidationFails_ReturnsError()
    {
        // Arrange
        var getByIdChecklist = BuildChecklist();
        _mockChecklistRepo.GetById(getByIdChecklist.Id).Returns(getByIdChecklist);

        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = "25"; // 25 is not the owner of the resultChecklist

        // Act
        var result = await _systemUnderTest.GetById(getByIdChecklist.Id);

        // Assert
        await _mockChecklistRepo.Received(1).GetById(getByIdChecklist.Id);
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
        var result = await _systemUnderTest.GetById(StubData.ChecklistId);

        // Assert
        await _mockChecklistRepo.DidNotReceive().GetById(StubData.ChecklistId);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.IncorrectInputData);
    }

    [Fact]
    public async Task GetById_WhenChecklistIdNotProvided_ReturnsError()
    {
        // Arrange
        string inputId = string.Empty;

        // Act
        var result = await _systemUnderTest.GetById(inputId);

        // Assert
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().GetById(default!);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.ChecklistIdNotSupplied);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetById_WhenChecklistNotFound_ReturnsError(bool performOwnershipValidation)
    {
        // Arrange
        _mockChecklistRepo.GetById(StubData.ChecklistId).Returns((Checklist) null!);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }
        // Act
        ////// If there is a userId then we should perform an Ownership Validation check
        var result = await _systemUnderTest.GetById(StubData.ChecklistId);

        // Assert
        await _mockChecklistRepo.Received(1).GetById(StubData.ChecklistId);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.EntityNotFound);
    }

    [Fact]
    public async Task GetUserChecklists_WhenEverythingIsOK_ReturnsListOfNotes()
    {
        // Arrange
        var foundChecklists = new List<Checklist> {
            BuildChecklist(checklistId: "1"),
            BuildChecklist(checklistId: "2"),
            BuildChecklist(checklistId: "3")
        };
        _mockChecklistRepo.Find(default).ReturnsForAnyArgs(foundChecklists);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = StubData.UserId;

        // Act
        var result = await _systemUnderTest.GetUserChecklists();

        // Assert
        await _mockChecklistRepo.ReceivedWithAnyArgs(1).Find(default);
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        var mappedResult = _mapper.Map<List<Checklist>>(result.Value);
        ObjectComparer.JsonCompare(foundChecklists, mappedResult).Should().BeTrue();
    }

    [Fact]
    public async Task GetUserChecklists_WhenUserIdNotProvided_ReturnsError()
    {
        // Arrange
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = string.Empty;

        // Act
        var result = await _systemUnderTest.GetUserChecklists();

        // Assert
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().Find(default);
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.UserIdNotSupplied);
    }

    internal record ChecklistDetailValueObject(
        string ChecklistId, string ParentDetailId, string TaskName, bool Status, List<ChecklistDetailValueObject>? SubItems
    );

    internal record ChecklistValueObject(string UserId, string Title, List<ChecklistDetailValueObject>? Details);

    [Fact]
    public async Task Create_WhenEverythingIsOK_ReturnsChecklist()
    {
        // Arrange
        var today = StubData.Today;
        var nextWeek = StubData.NextWeek;
        var inputData = BuildChecklist(creationDate: today, modificationDate: nextWeek);
        var mappedInputData = _mapper.Map<ChecklistDTO>(inputData);
        Func<Checklist, bool> repoCreateChecklistValidator = arg => 
            ObjectComparer.JsonTransformAndCompare<Checklist, ChecklistValueObject>(arg, inputData);
        var flattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(inputData.Id, mappedInputData.Details);
        // string[] propertiesToIgnoreChecklistDetail = [
        //     nameof(ChecklistDetail.Id), nameof(ChecklistDetail.ChecklistId), nameof(ChecklistDetail.ParentDetailId),
        //     nameof(ChecklistDetail.CreationDate), nameof(ChecklistDetail.ModificationDate)
        // ];

        foreach (var d in flattenedDetails)
        {
            var detail = _mapper.Map<ChecklistDetail>(d);
            _mockChecklistDetailsRepo.Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == detail.TaskName && arg.Status == detail.Status), false
            ).Returns(Task.CompletedTask);
        }

        _mockChecklistRepo.Create(Arg.Is<Checklist>(arg => repoCreateChecklistValidator(arg)), false)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.Create(mappedInputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeNull();

        await _mockUnitOfWork.Received(1).StartTransaction();
        await _mockChecklistRepo.Received(1).Create(Arg.Is<Checklist>(arg => repoCreateChecklistValidator(arg)), false);

        foreach (var d in flattenedDetails)
        {
            var detail = _mapper.Map<ChecklistDetail>(d);
            await _mockChecklistDetailsRepo.Received(1).Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == detail.TaskName && arg.Status == detail.Status), false
            );
        }

        var mappedResult = _mapper.Map<Checklist>(result.Value.Entity);
        await _mockUnitOfWork.Received(1).Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
        ObjectComparer.JsonTransformAndCompare<Checklist, ChecklistValueObject>(inputData, mappedResult).Should().BeTrue();
    }

    [Fact]
    public async Task Create_WhenEverythingIsOKAndEmptyDetails_ReturnsChecklist()
    {
        // Arrange
        var today = StubData.Today;
        var nextWeek = StubData.NextWeek;
        var inputData = BuildChecklist(creationDate: today, modificationDate: nextWeek, details: []);
        var mappedInputData = _mapper.Map<ChecklistDTO>(inputData);
        Func<Checklist, bool> repoCreateChecklistValidator = arg => 
            ObjectComparer.JsonTransformAndCompare<Checklist, ChecklistValueObject>(arg, inputData);

        _mockChecklistRepo.Create(Arg.Is<Checklist>(arg => repoCreateChecklistValidator(arg)), false)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.Create(mappedInputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeNull();

        await _mockUnitOfWork.Received(1).StartTransaction();
        await _mockChecklistRepo.Received(1).Create(Arg.Is<Checklist>(arg => repoCreateChecklistValidator(arg)), false);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Create(default!, default);
        var mappedResult = _mapper.Map<Checklist>(result.Value.Entity);
        await _mockUnitOfWork.Received(1).Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
        ObjectComparer.JsonTransformAndCompare<Checklist, ChecklistValueObject>(inputData, mappedResult).Should().BeTrue();
    }

    [Fact]
    public async Task Create_WhenAnExceptionIsThrownDuringTransaction_ReturnsError()
    {
        // Arrange
        var thrownException = new Exception(ErrorCodes.UnknownError);
        var today = StubData.Today;
        var nextWeek = StubData.NextWeek;
        var inputData = BuildChecklist(creationDate: today, modificationDate: nextWeek);
        var mappedInputData = _mapper.Map<ChecklistDTO>(inputData);
        Func<Checklist, bool> repoCreateChecklistValidator = arg => 
            ObjectComparer.JsonTransformAndCompare<Checklist, ChecklistValueObject>(arg, inputData);
        var flattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(inputData.Id, mappedInputData.Details);
        // string[] propertiesToIgnoreChecklistDetail = [
        //     nameof(ChecklistDetail.Id), nameof(ChecklistDetail.ChecklistId), nameof(ChecklistDetail.ParentDetailId),
        //     nameof(ChecklistDetail.CreationDate), nameof(ChecklistDetail.ModificationDate)
        // ];

        foreach (var d in flattenedDetails)
        {
            var detail = _mapper.Map<ChecklistDetail>(d);
            _mockChecklistDetailsRepo.Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == detail.TaskName && arg.Status == detail.Status), false
            ).Returns(Task.CompletedTask);
        }

        _mockChecklistRepo.Create(Arg.Is<Checklist>(arg => repoCreateChecklistValidator(arg)), false)
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.When(m => m.Commit()).Throw(thrownException);

        // Act
        var result = await _systemUnderTest.Create(mappedInputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        // result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.RepoProblem);

        await _mockUnitOfWork.Received(1).StartTransaction();
        await _mockChecklistRepo.Received(1).Create(Arg.Is<Checklist>(arg => repoCreateChecklistValidator(arg)), false);

        foreach (var d in flattenedDetails)
        {
            var detail = _mapper.Map<ChecklistDetail>(d);
            await _mockChecklistDetailsRepo.Received(1).Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == detail.TaskName && arg.Status == detail.Status), false
            );
        }

        await _mockUnitOfWork.Received(1).Commit();
        _mockLogger.Received(1).LogError(thrownException, ErrorMessages.DbTransactionError);
        await _mockUnitOfWork.Received(1).Rollback();
    }

    [Fact]
    public async Task Create_WhenUserIdNotProvided_ReturnsError()
    {
        // Arrange
        var inputData = BuildChecklist(userId: string.Empty);

        // Act
        var result = await _systemUnderTest.Create(_mapper.Map<ChecklistDTO>(inputData));

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Entity.Should().BeNull();
        result.Value.Id.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.UserIdNotSupplied);

        await _mockUnitOfWork.Received(1).StartTransaction();
        await _mockChecklistRepo.DidNotReceive().Create(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceive().Create(default!, default);

        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.Received(1).Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError((Exception) default!, default);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Create_WhenChecklistTitleNotProvided_ReturnsError(string? checklistTitle)
    {
        // Arrange
        var inputData = BuildChecklist(title: checklistTitle!);

        // Act
        var result = await _systemUnderTest.Create(_mapper.Map<ChecklistDTO>(inputData));

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Entity.Should().BeNull();
        result.Value.Id.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.NoteTitleNotFound);

        await _mockUnitOfWork.Received(1).StartTransaction();
        await _mockChecklistRepo.DidNotReceive().Create(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceive().Create(default!, default);

        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.Received(1).Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError((Exception) default!, default);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteById_WhenEverythingIsOK_ReturnsSuccess(bool performOwnershipValidation)
    {
        // Arrange
        string checklistUserId = performOwnershipValidation ? StubData.UserId : "100";
        var getByIdChecklist = BuildChecklist(userId: checklistUserId);
        _mockChecklistRepo.GetById(StubData.ChecklistId).Returns(getByIdChecklist);
        _mockChecklistRepo.DeleteById(StubData.ChecklistId, false).Returns(Task.CompletedTask);
        _mockChecklistRepo.DeleteDetails(StubData.ChecklistId, false).Returns(1);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        ////// If there is a userId then we should perform an Ownership Validation check
        var result = await _systemUnderTest.DeleteById(StubData.ChecklistId);

        // Assert
        result.Should().BeNull();

        await _mockChecklistRepo.Received(1).DeleteDetails(StubData.ChecklistId, false);
        await _mockChecklistRepo.Received(1).GetById(StubData.ChecklistId);
        await _mockChecklistRepo.Received(1).DeleteById(StubData.ChecklistId, false);

        await _mockUnitOfWork.Received(1).StartTransaction();
        await _mockUnitOfWork.Received(1).Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError((Exception) default!, default);
    }

    [Fact]
    public async Task DeleteById_WhenAnExceptionIsThrownDuringTransaction_ReturnsError()
    {
        // Arrange
        var thrownException = new Exception(ErrorCodes.UnknownError);
        var getByIdChecklist = BuildChecklist(checklistId: StubData.ChecklistId);
        _mockChecklistRepo.GetById(StubData.ChecklistId).Returns(getByIdChecklist);
        _mockChecklistRepo.DeleteDetails(StubData.ChecklistId, false).Returns(1);
        _mockChecklistRepo.DeleteById(StubData.ChecklistId, false).Returns(Task.CompletedTask);
        _mockUnitOfWork.When(m => m.Commit()).Throw(thrownException);


        // Act
        var result = await _systemUnderTest.DeleteById(StubData.ChecklistId);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be(ErrorCodes.RepoProblem);

        await _mockChecklistRepo.Received(1).GetById(StubData.ChecklistId);
        await _mockChecklistRepo.Received(1).DeleteDetails(StubData.ChecklistId, false);
        await _mockChecklistRepo.Received(1).DeleteById(StubData.ChecklistId, false);
        await _mockUnitOfWork.Received(1).StartTransaction();
        await _mockUnitOfWork.Received(1).Commit();
        await _mockUnitOfWork.Received(1).Rollback();
        _mockLogger.Received(1).LogError(thrownException, ErrorMessages.DbTransactionError);
    }

    [Fact]
    public async Task DeleteById_WhenChecklistIdNotProvided_ReturnsError()
    {
        // Arrange
        string inputId = string.Empty;

        // Act
        var result = await _systemUnderTest.DeleteById(inputId);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be(ErrorCodes.ChecklistIdNotSupplied);

        await _mockChecklistRepo.DidNotReceive().GetById(StubData.ChecklistId);
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().DeleteById(default!, default);
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().DeleteDetails(default!, default);
        await _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError((Exception) default!, default);
    }

    [Fact]
    public async Task DeleteById_WhenChecklistDoesNotBelongToUser_ReturnsError()
    {
        // Arrange
        var getByIdChecklist = BuildChecklist(userId: "100"); // 100 is not the user making the request
        _mockChecklistRepo.GetById(StubData.ChecklistId).Returns(getByIdChecklist);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = StubData.UserId;

        // Act
        var result = await _systemUnderTest.DeleteById(StubData.ChecklistId);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be(ErrorCodes.EntityDoesNotBelongToUser);

        await _mockChecklistRepo.Received(1).GetById(StubData.ChecklistId);
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().DeleteDetails(default!, default);
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().DeleteById(default!, default);
        await _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError((Exception) default!, default);
    }

    [Fact]
    public async Task DeleteById_WhenChecklistNotFound_ReturnsError()
    {
        // Arrange
        _mockChecklistRepo.GetById(StubData.ChecklistId).Returns((Checklist) default!);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = StubData.UserId;

        // Act
        var result = await _systemUnderTest.DeleteById(StubData.ChecklistId);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be(ErrorCodes.EntityNotFound);

        await _mockChecklistRepo.Received(1).GetById(StubData.ChecklistId);
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().DeleteDetails(default!, default);
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().DeleteById(default!, default);
        await _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError((Exception) default!, default);
    }

    // [Fact]
    // public async Task Delete_WhenNoteDoesNotBelongToUser_ReturnsError()
    // {
    //     // Arrange
    //     var getByIdNote = BuildNote(userId: "25");
    //     _mockChecklistRepo.GetById(StubData.NoteId).Returns(getByIdNote);
    //     _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
    //     _systemUnderTest.CrudSettings.UserId = StubData.UserId;

    //     // Act
    //     var result = await _systemUnderTest.DeleteById(StubData.NoteId);

    //     // Assert
    //     await _mockChecklistRepo.Received(1).GetById(StubData.NoteId);
    //     await _mockChecklistRepo.DidNotReceive().DeleteById(default!, default);
    //     result.Should().NotBeNull();
    //     result!.Code.Should().Be(ErrorCodes.EntityDoesNotBelongToUser);
    // }

    // [Theory]
    // [InlineData(true)]
    // [InlineData(false)]
    // public async Task Update_WhenEverythingIsOK_ReturnsModifiedNote(bool performOwnershipValidation)
    // {
    //     // Arrange
    //     string noteUserId = performOwnershipValidation ? StubData.UserId : "100";
    //     var getByIdNote = BuildNote(userId: noteUserId);
    //     // var resultNote = inputData.ToEntity();
    //     _mockChecklistRepo.GetById(StubData.NoteId).Returns(getByIdNote);

    //     string[] comparisonIgnoredProperties = [nameof(Note.Id), nameof(Note.CreationDate), nameof(Note.ModificationDate)];
    //     Func<Note, bool> repoUpdateNoteValidator = arg => ObjectComparer.JsonCompare(
    //         arg, getByIdNote, comparisonIgnoredProperties
    //     );

    //     _mockChecklistRepo.Update(Arg.Is<Note>(arg => repoUpdateNoteValidator(arg)), true).Returns(Task.CompletedTask);
    //     _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
    //     if (performOwnershipValidation) {
    //         _systemUnderTest.CrudSettings.UserId = StubData.UserId;
    //     }

    //     // Act
    //     ////// If there is a userId then we should perform an Ownership Validation check
    //     var result = await _systemUnderTest.Update(StubData.NoteId, _mapper.Map<NoteDTO>(getByIdNote));

    //     // Assert
    //     await _mockChecklistRepo.Received(1).GetById(StubData.NoteId);
    //     await _mockChecklistRepo.Received(1).Update(Arg.Is<Note>(arg => repoUpdateNoteValidator(arg)), true);
    //     result.Should().NotBeNull();
    //     result.Error.Should().BeNull();
    //     result.Value.Should().NotBeNull();
    //     result.Value!.ModificationDate.Should().NotBeNull();
    //     var mappedResult = _mapper.Map<Note>(result.Value);
    //     ObjectComparer.JsonCompare(getByIdNote, mappedResult, comparisonIgnoredProperties).Should().BeTrue();
    // }

    // [Fact]
    // public async Task Update_WhenUserIdNotProvided_ReturnsError()
    // {
    //     // Arrange
    //     var inputData = BuildNote(userId: string.Empty);

    //     // Act
    //     var result = await _systemUnderTest.Update(StubData.NoteId, _mapper.Map<NoteDTO>(inputData));

    //     // Assert
    //     await _mockChecklistRepo.DidNotReceive().GetById(StubData.NoteId);
    //     await _mockChecklistRepo.DidNotReceive().Update(default!, default);
    //     result.Should().NotBeNull();
    //     result.Error.Should().NotBeNull();
    //     result.Value.Should().BeNull();
    //     result.Error!.Code.Should().Be(ErrorCodes.UserIdNotSupplied);
    // }

    // [Theory]
    // [InlineData("")]
    // [InlineData(null)]
    // [InlineData("   ")]
    // public async Task Update_WhenNoteTitleNotProvided_ReturnsError(string? noteTitle)
    // {
    //     // Arrange
    //     var inputData = BuildNote(title: noteTitle!);

    //     // Act
    //     var result = await _systemUnderTest.Update(StubData.NoteId, _mapper.Map<NoteDTO>(inputData));

    //     // Assert
    //     await _mockChecklistRepo.DidNotReceive().GetById(StubData.NoteId);
    //     await _mockChecklistRepo.DidNotReceive().Update(default!, default);
    //     result.Should().NotBeNull();
    //     result.Error.Should().NotBeNull();
    //     result.Value.Should().BeNull();
    //     result.Error!.Code.Should().Be(ErrorCodes.NoteTitleNotFound);
    // }

    // [Theory]
    // [InlineData(true)]
    // [InlineData(false)]
    // public async Task Update_WhenNoteNotFound_ReturnsError(bool performOwnershipValidation)
    // {
    //     // Arrange
    //     var inputData = BuildNote(noteId: string.Empty);
    //     _mockChecklistRepo.GetById(StubData.NoteId).Returns((Note) default!);
    //     _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
    //     if (performOwnershipValidation) {
    //         _systemUnderTest.CrudSettings.UserId = StubData.UserId;
    //     }

    //     // Act
    //     var result = await _systemUnderTest.Update(StubData.NoteId, _mapper.Map<NoteDTO>(inputData));

    //     // Assert
    //     await _mockChecklistRepo.Received(1).GetById(StubData.NoteId);
    //     await _mockChecklistRepo.DidNotReceive().Update(default!, default);
    //     result.Should().NotBeNull();
    //     result.Error.Should().NotBeNull();
    //     result.Value.Should().BeNull();
    //     result.Error!.Code.Should().Be(ErrorCodes.EntityNotFound);
    // }

    // [Fact]
    // public async Task Update_WhenNoteDoesNotBelongToUser_ReturnsError()
    // {
    //     // Arrange
    //     var inputData = BuildNote(noteId: string.Empty);
    //     var resultNote = BuildNote(userId: "25");
    //     _mockChecklistRepo.GetById(StubData.NoteId).Returns(resultNote);
    //     _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
    //     _systemUnderTest.CrudSettings.UserId = StubData.UserId;

    //     // Act
    //     var result = await _systemUnderTest.Update(StubData.NoteId, _mapper.Map<NoteDTO>(inputData));

    //     // Assert
    //     await _mockChecklistRepo.Received(1).GetById(StubData.NoteId);
    //     await _mockChecklistRepo.DidNotReceive().Update(default!, default);
    //     result.Should().NotBeNull();
    //     result.Error.Should().NotBeNull();
    //     result.Value.Should().BeNull();
    //     result.Error!.Code.Should().Be(ErrorCodes.EntityDoesNotBelongToUser);
    // }


    private Checklist BuildChecklist(
        string checklistId = StubData.ChecklistId, string userId = StubData.UserId, string title = StubData.ChecklistTitle,
        DateTime? creationDate = null, DateTime? modificationDate = null, List<ChecklistDetail>? details = null
    ) {
        details ??= BuildRandomDetails(checklistId);

        return new Checklist {
            Id = checklistId,
            UserId = userId,
            Title = title,
            Details = details,
            CreationDate = creationDate ?? StubData.Today,
            ModificationDate = modificationDate ?? StubData.NextWeek
        };
    }

    private List<ChecklistDetail>? BuildRandomDetails(string checklistId, int depth = 0, string? parentDetailId = null)
    {
        var details = new List<ChecklistDetail>();
        int detailsNumber = new Random().Next(depth == 0 ? 1 : 0, 5 - depth);
        for (int i = 0; i < detailsNumber; i++)
        {
            var detail = BuildChecklistDetail(
                checklistId: checklistId, taskName: StubData.CreateRandomString(), parentDetailId: parentDetailId
            );
            detail.SubItems = BuildRandomDetails(checklistId, depth + 1, detail.Id);
            // detail.SubItems = [];
            details.Add(detail);
        }
        return details;
    }

    private ChecklistDetail BuildChecklistDetail(
        string? detailId = null, string checklistId = StubData.ChecklistId, string taskName = StubData.ChecklistTaskName,
        string? parentDetailId = null, bool status = true, DateTime? creationDate = null, DateTime? modificationDate = null
    ) {
        return new ChecklistDetail {
            Id = string.IsNullOrWhiteSpace(detailId) ? Guid.CreateVersion7().ToString("N") : detailId,
            ChecklistId = checklistId,
            TaskName = taskName,
            ParentDetailId = parentDetailId,
            Status = status,
            CreationDate = creationDate ?? StubData.Today,
            ModificationDate = modificationDate ?? StubData.NextWeek
        };
    }
}
