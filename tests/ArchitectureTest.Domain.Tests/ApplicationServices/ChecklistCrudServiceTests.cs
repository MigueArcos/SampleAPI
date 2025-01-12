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
    private readonly Random _random = new();

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
        var flattenedDetails = ApplicationModelsMappingProfile
            .FlattenAndGenerateChecklistDetails(inputData.Id, inputData.Details);
        // string[] propertiesToIgnoreChecklistDetail = [
        //     nameof(ChecklistDetail.Id), nameof(ChecklistDetail.ChecklistId), nameof(ChecklistDetail.ParentDetailId),
        //     nameof(ChecklistDetail.CreationDate), nameof(ChecklistDetail.ModificationDate)
        // ];

        flattenedDetails.ForEach(d => {
            _mockChecklistDetailsRepo.Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == d.TaskName && arg.Status == d.Status), false
            ).Returns(Task.CompletedTask);
        });

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
            await _mockChecklistDetailsRepo.Received(1).Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == d.TaskName && arg.Status == d.Status), false
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
        var flattenedDetails = ApplicationModelsMappingProfile
            .FlattenAndGenerateChecklistDetails(inputData.Id, inputData.Details);
        // string[] propertiesToIgnoreChecklistDetail = [
        //     nameof(ChecklistDetail.Id), nameof(ChecklistDetail.ChecklistId), nameof(ChecklistDetail.ParentDetailId),
        //     nameof(ChecklistDetail.CreationDate), nameof(ChecklistDetail.ModificationDate)
        // ];

        flattenedDetails.ForEach(d => {
            _mockChecklistDetailsRepo.Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == d.TaskName && arg.Status == d.Status), false
            ).Returns(Task.CompletedTask);
        });

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
            await _mockChecklistDetailsRepo.Received(1).Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == d.TaskName && arg.Status == d.Status), false
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

    // Update process is the longest one, it's composed of these steps:
        // 1. StartTransaction
        // 2. GetById
        // 3. Flatten and Validate DetailsToUpdate
        // 4. Validate DetailsToDelete (If one detail in both UpdateAndDelete, Delete it and remove it from Update)
        // 5. Flatten DetailsToAdd
        // 6. Insert DetailsToAdd
        // 7. Update DetailsToUpdate
        // 8. Delete DetailsToDelete
        // 9. Update Parent
        // 10. Commit
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_WhenEverythingIsOK_ReturnsChecklist(bool performOwnershipValidation)
    {
        // Arrange
        var inputId = StubData.ChecklistId;
        var getByIdChecklist = BuildChecklist(checklistId: inputId);
        var oldFlattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(getByIdChecklist.Details);
        // take n random items to update and also n random items to delete from getByIdChecklist
        var (detailsToUpdate, detailsToDelete) = PickRandomDetails(oldFlattenedDetails);
        detailsToUpdate.ForEach(d => {
            d.TaskName = StubData.CreateRandomString();
            d.Status = RandomBool();
        });

        var inputData = BuildUpdateChecklistModel(
            checklistId: inputId, detailsToUpdate: detailsToUpdate, detailsToDelete: detailsToDelete
        );
        var actualDetailsToDelete = ApplicationModelsMappingProfile.FindAllDetailsToRemove(oldFlattenedDetails, detailsToDelete);
        var actualDetailsToUpdate = inputData.ProcessDetailsToUpdate(actualDetailsToDelete);

        Func<Checklist, bool> repoUpdateChecklistValidator = arg => 
            arg.Id == inputData.Id && arg.UserId == inputData.UserId &&
            arg.Title == inputData.Title && arg.Details?.Count == 0;

        var flattenedDetailsToAdd = ApplicationModelsMappingProfile.FlattenAndGenerateChecklistDetails(
            inputId, _mapper.Map<List<ChecklistDetail>>(inputData.DetailsToAdd)
        );

        _mockChecklistRepo.GetById(inputId).Returns(getByIdChecklist);

        _mockChecklistRepo.Update(Arg.Is<Checklist>(arg => repoUpdateChecklistValidator(arg)), false)
            .Returns(Task.CompletedTask);

        actualDetailsToDelete?.ForEach(id => {
            _mockChecklistDetailsRepo.DeleteById(id).Returns(Task.CompletedTask);
        });
        
        actualDetailsToUpdate?.ForEach(d => {
            _mockChecklistDetailsRepo.Update(
                Arg.Is<ChecklistDetail>(arg =>
                    arg.TaskName == d.TaskName && arg.Status == d.Status && arg.Id == d.Id && 
                    arg.ChecklistId == d.ChecklistId && arg.ParentDetailId == d.ParentDetailId
                ), false
            ).Returns(Task.CompletedTask);
        });

        flattenedDetailsToAdd.ForEach(d => {
            _mockChecklistDetailsRepo.Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == d.TaskName && arg.Status == d.Status), false
            ).Returns(Task.CompletedTask);
        });
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        var result = await _systemUnderTest.Update(inputId, inputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().NotBeNull();
        result.Value!.Should().BeOfType<UpdateChecklistDTO>();

        await _mockChecklistRepo.Received(1).GetById(inputId);
        await _mockUnitOfWork.Received(1).StartTransaction();
        await _mockChecklistRepo.Received(1).Update(Arg.Is<Checklist>(arg => repoUpdateChecklistValidator(arg)), false);

        foreach (var id in actualDetailsToDelete!)
        {
            await _mockChecklistDetailsRepo.Received(1).DeleteById(id, false);
        }

        foreach (var d in actualDetailsToUpdate!)
        {
            await _mockChecklistDetailsRepo.Received(1).Update(
                Arg.Is<ChecklistDetail>(arg =>
                    arg.TaskName == d.TaskName && arg.Status == d.Status && arg.Id == d.Id && 
                    arg.ChecklistId == d.ChecklistId && arg.ParentDetailId == d.ParentDetailId
                ), false
            );
        }

        foreach (var d in flattenedDetailsToAdd)
        {
            await _mockChecklistDetailsRepo.Received(1).Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == d.TaskName && arg.Status == d.Status), false
            );
        }

        var mappedResult = result.Value as UpdateChecklistDTO;
        await _mockUnitOfWork.Received(1).Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
        // ObjectComparer.JsonTransformAndCompare<UpdateChecklistDTO, UpdateChecklistDTOValueObject>(inputData, mappedResult!)
        //     .Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_WhenAnExceptionIsThrownDuringTransaction_ReturnsError(bool performOwnershipValidation)
    {
        // Arrange
        var thrownException = new Exception(ErrorCodes.UnknownError);
        var inputId = StubData.ChecklistId;
        var getByIdChecklist = BuildChecklist(checklistId: inputId);
        var oldFlattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(getByIdChecklist.Details);
        // take n random items to update and also n random items to delete from getByIdChecklist
        var (detailsToUpdate, detailsToDelete) = PickRandomDetails(oldFlattenedDetails);
        detailsToUpdate.ForEach(d => {
            d.TaskName = StubData.CreateRandomString();
            d.Status = RandomBool();
        });

        var inputData = BuildUpdateChecklistModel(
            checklistId: inputId, detailsToUpdate: detailsToUpdate, detailsToDelete: detailsToDelete
        );
        var actualDetailsToDelete = ApplicationModelsMappingProfile.FindAllDetailsToRemove(oldFlattenedDetails, detailsToDelete);
        var actualDetailsToUpdate = inputData.ProcessDetailsToUpdate(actualDetailsToDelete);
        
        Func<Checklist, bool> repoUpdateChecklistValidator = arg => 
            arg.Id == inputData.Id && arg.UserId == inputData.UserId &&
            arg.Title == inputData.Title && arg.Details?.Count == 0;

        var flattenedDetailsToAdd = ApplicationModelsMappingProfile.FlattenAndGenerateChecklistDetails(
            inputId, _mapper.Map<List<ChecklistDetail>>(inputData.DetailsToAdd)
        );

        _mockChecklistRepo.GetById(inputId).Returns(getByIdChecklist);

        _mockChecklistRepo.Update(Arg.Is<Checklist>(arg => repoUpdateChecklistValidator(arg)), false)
            .Returns(Task.CompletedTask);

        actualDetailsToDelete?.ForEach(id => {
            _mockChecklistDetailsRepo.DeleteById(id).Returns(Task.CompletedTask);
        });
        
        actualDetailsToUpdate?.ForEach(d => {
            _mockChecklistDetailsRepo.Update(
                Arg.Is<ChecklistDetail>(arg =>
                    arg.TaskName == d.TaskName && arg.Status == d.Status && arg.Id == d.Id && 
                    arg.ChecklistId == d.ChecklistId && arg.ParentDetailId == d.ParentDetailId
                ), false
            ).Returns(Task.CompletedTask);
        });

        flattenedDetailsToAdd.ForEach(d => {
            _mockChecklistDetailsRepo.Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == d.TaskName && arg.Status == d.Status), false
            ).Returns(Task.CompletedTask);
        });
        _mockUnitOfWork.When(m => m.Commit()).Throw(thrownException);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        var result = await _systemUnderTest.Update(inputId, inputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.RepoProblem);

        await _mockChecklistRepo.Received(1).GetById(inputId);
        await _mockUnitOfWork.Received(1).StartTransaction();
        await _mockChecklistRepo.Received(1).Update(Arg.Is<Checklist>(arg => repoUpdateChecklistValidator(arg)), false);

        foreach (var id in actualDetailsToDelete!)
        {
            await _mockChecklistDetailsRepo.Received(1).DeleteById(id, false);
        }

        foreach (var d in actualDetailsToUpdate!)
        {
            await _mockChecklistDetailsRepo.Received(1).Update(
                Arg.Is<ChecklistDetail>(arg =>
                    arg.TaskName == d.TaskName && arg.Status == d.Status && arg.Id == d.Id && 
                    arg.ChecklistId == d.ChecklistId && arg.ParentDetailId == d.ParentDetailId
                ), false
            );
        }

        foreach (var d in flattenedDetailsToAdd)
        {
            await _mockChecklistDetailsRepo.Received(1).Create(
                Arg.Is<ChecklistDetail>(arg => arg.TaskName == d.TaskName && arg.Status == d.Status), false
            );
        }

        await _mockUnitOfWork.Received(1).Commit();
        await _mockUnitOfWork.Received(1).Rollback();
        _mockLogger.Received(1).LogError(thrownException, ErrorMessages.DbTransactionError);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_WhenOneChecklistDetailToDeleteDoesNotExists_ReturnsError(bool performOwnershipValidation)
    {
        // Arrange
        var inputId = StubData.ChecklistId;
        var getByIdChecklist = BuildChecklist(checklistId: inputId);
        var oldFlattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(getByIdChecklist.Details);
        // take n random items to update and also n random items to delete from getByIdChecklist
        var (detailsToUpdate, detailsToDelete) = PickRandomDetails(oldFlattenedDetails);
        detailsToDelete.Add(StubData.CreateRandomString());
        detailsToUpdate.ForEach(d => {
            d.TaskName = StubData.CreateRandomString();
            d.Status = RandomBool();
        });

        var inputData = BuildUpdateChecklistModel(
            checklistId: inputId, detailsToUpdate: detailsToUpdate, detailsToDelete: detailsToDelete
        );
        var actualDetailsToDelete = ApplicationModelsMappingProfile.FindAllDetailsToRemove(oldFlattenedDetails, detailsToDelete);
        var actualDetailsToUpdate = inputData.ProcessDetailsToUpdate(actualDetailsToDelete);

        _mockChecklistRepo.GetById(inputId).Returns(getByIdChecklist);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        var result = await _systemUnderTest.Update(inputId, inputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.OneOrMoreChecklistDetailToDeleteNotFound);

        await _mockChecklistRepo.Received(1).GetById(inputId);
        await _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().DeleteById(default!, false);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Create(default!, default);

        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_WhenOneChecklistDetailToUpdateDoesNotExists_ReturnsError(bool performOwnershipValidation)
    {
        // Arrange
        var inputId = StubData.ChecklistId;
        var getByIdChecklist = BuildChecklist(checklistId: inputId);
        var oldFlattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(getByIdChecklist.Details);
        // take n random items to update and also n random items to delete from getByIdChecklist
        var (detailsToUpdate, detailsToDelete) = PickRandomDetails(oldFlattenedDetails);
        detailsToUpdate.Add(BuildChecklistDetail());
        detailsToUpdate.ForEach(d => {
            d.TaskName = StubData.CreateRandomString();
            d.Status = RandomBool();
        });

        var inputData = BuildUpdateChecklistModel(
            checklistId: inputId, detailsToUpdate: detailsToUpdate, detailsToDelete: detailsToDelete
        );

        _mockChecklistRepo.GetById(inputId).Returns(getByIdChecklist);
        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        var result = await _systemUnderTest.Update(inputId, inputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.OneOrMoreChecklistDetailToUpdateNotFound);

        await _mockChecklistRepo.Received(1).GetById(inputId);
        await _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().DeleteById(default!, false);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Create(default!, default);

        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_WhenUserIdNotProvided_ReturnsError(bool performOwnershipValidation)
    {
        // Arrange
        var inputId = StubData.ChecklistId;

        var inputData = BuildUpdateChecklistModel(
            checklistId: inputId, userId: string.Empty
        );

        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        var result = await _systemUnderTest.Update(inputId, inputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.UserIdNotSupplied);

        await _mockChecklistRepo.DidNotReceive().GetById(inputId);
        await _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().DeleteById(default!, false);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Create(default!, default);

        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_WhenChecklistTitleNotProvided_ReturnsError(bool performOwnershipValidation)
    {
        // Arrange
        var inputId = StubData.ChecklistId;

        var inputData = BuildUpdateChecklistModel(
            checklistId: inputId, title: string.Empty
        );

        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        if (performOwnershipValidation) {
            _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        }

        // Act
        var result = await _systemUnderTest.Update(inputId, inputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.ChecklistTitleNotFound);

        await _mockChecklistRepo.DidNotReceive().GetById(inputId);
        await _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().DeleteById(default!, false);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Create(default!, default);

        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Fact]
    public async Task Update_WhenChecklistDoesNotBelongToUser_ReturnsError()
    {
        // Arrange
        var inputId = StubData.ChecklistId;
        var getByIdChecklist = BuildChecklist(checklistId: inputId, userId: "250"); // this is not the owner of this Checklist
        var oldFlattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(getByIdChecklist.Details);
        // take n random items to update and also n random items to delete from getByIdChecklist
        var (detailsToUpdate, detailsToDelete) = PickRandomDetails(oldFlattenedDetails);
        detailsToUpdate.ForEach(d => {
            d.TaskName = StubData.CreateRandomString();
            d.Status = RandomBool();
        });

        var inputData = BuildUpdateChecklistModel(
            checklistId: inputId, detailsToUpdate: detailsToUpdate, detailsToDelete: detailsToDelete
        );

        _mockChecklistRepo.GetById(inputId).Returns(getByIdChecklist);

        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = true;
        _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        

        // Act
        var result = await _systemUnderTest.Update(inputId, inputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.EntityDoesNotBelongToUser);

        await _mockChecklistRepo.Received(1).GetById(inputId);
        await _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().DeleteById(default!, false);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Create(default!, default);

        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_WhenChecklistNotFound_ReturnsError(bool performOwnershipValidation)
    {
        // Arrange
        var inputId = StubData.ChecklistId;

        _mockChecklistRepo.GetById(inputId).Returns((Checklist) default!);

        _systemUnderTest.CrudSettings.ValidateEntityBelongsToUser = performOwnershipValidation;
        _systemUnderTest.CrudSettings.UserId = StubData.UserId;
        var inputData = BuildUpdateChecklistModel(checklistId: inputId);
        

        // Act
        var result = await _systemUnderTest.Update(inputId, inputData);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be(ErrorCodes.EntityNotFound);

        await _mockChecklistRepo.Received(1).GetById(inputId);
        await _mockUnitOfWork.DidNotReceive().StartTransaction();
        await _mockChecklistRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().DeleteById(default!, false);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Update(default!, default);
        await _mockChecklistDetailsRepo.DidNotReceiveWithAnyArgs().Create(default!, default);

        await _mockUnitOfWork.DidNotReceive().Commit();
        await _mockUnitOfWork.DidNotReceive().Rollback();
        _mockLogger.DidNotReceiveWithAnyArgs().LogError(default);
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

    internal record ChecklistDetailValueObject(
        string ChecklistId, string ParentDetailId, string TaskName, bool Status, List<ChecklistDetailValueObject>? SubItems
    );

    internal record ChecklistValueObject(string UserId, string Title, List<ChecklistDetailValueObject>? Details);

    internal record UpdateChecklistDTOValueObject(
        string UserId, string Title, List<ChecklistDetailValueObject>? DetailsToAdd,
        List<ChecklistDetailValueObject>? DetailsToUpdate, List<string>? DetailsToDelete
    );


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

    private UpdateChecklistDTO BuildUpdateChecklistModel(
        string checklistId = StubData.ChecklistId, string userId = StubData.UserId,
        string title = StubData.ChecklistTitle, List<ChecklistDetail>? detailsToAdd = null,
        List<ChecklistDetail>? detailsToUpdate = null, List<string>? detailsToDelete = null
    ) {
        detailsToAdd ??= BuildRandomDetails(checklistId);
        // detailsToUpdate ??= BuildRandomDetails(checklistId);
        // detailsToDelete ??= 

        return new UpdateChecklistDTO {
            Id = checklistId,
            UserId = userId,
            Title = title,
            DetailsToAdd = _mapper.Map<List<ChecklistDetailDTO>>(detailsToAdd),
            DetailsToUpdate = _mapper.Map<List<ChecklistDetailDTO>>(detailsToUpdate),
            DetailsToDelete = detailsToDelete
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

    private bool RandomBool() => _random.NextDouble() >= 0.5;

    private (List<ChecklistDetail> DetailToUpdate, List<string> DetailsToDelete) PickRandomDetails(
        List<ChecklistDetail> flattenedDetails
    ){
        var detailsToUpdate = new List<ChecklistDetail>();
        var detailsToDelete = new List<string>();
    
        flattenedDetails.ForEach(detail => {
            if (RandomBool())
                detailsToUpdate.Add(detail);
            if (RandomBool())
                detailsToDelete.Add(detail.Id);
        });
        
        return (detailsToUpdate, detailsToDelete);
    }
}
