using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService;

using ValidationFunc = (Func<ChecklistDTO?, string?, bool>, string);

public class ChecklistCrudService : EntityCrudService<Checklist, ChecklistDTO>, IChecklistCrudService
{
    private readonly Dictionary<CrudOperation, List<ValidationFunc>> _validations;
    private readonly ILogger<ChecklistCrudService> _logger;
    private readonly IRepository<ChecklistDetail> _checklistDetailsRepo;
    private readonly IChecklistRepository _checklistRepo;

    public override Dictionary<CrudOperation, List<ValidationFunc>>ValidationsByOperation => _validations;

    public ChecklistCrudService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ChecklistCrudService> logger)
        : base(unitOfWork, mapper)
    {
        _logger = logger;
        _checklistDetailsRepo = _unitOfWork.Repository<ChecklistDetail>();
        _checklistRepo = (IChecklistRepository) _unitOfWork.Repository<Checklist>();
        _validations = new (){
            [CrudOperation.Create] = [
                ((input, entityId) => input is null, ErrorCodes.InputDataNotFound),
                ((input, entityId) => string.IsNullOrWhiteSpace(input!.UserId), ErrorCodes.UserIdNotSupplied),
                ((input, entityId) => 
                    input!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                    ErrorCodes.CannotCreateDataForThisUserId
                ),
                ((input, entityId) => string.IsNullOrWhiteSpace(input!.Title), ErrorCodes.NoteTitleNotFound),
            ],
            [CrudOperation.ReadById] = [
                ((input, entityId) => string.IsNullOrWhiteSpace(entityId), ErrorCodes.ChecklistIdNotSupplied)
            ],
            [CrudOperation.Update] = [
                ((input, entityId) => entityId == null, ErrorCodes.ChecklistIdNotSupplied),
                ((input, entityId) => input is null, ErrorCodes.InputDataNotFound),
                ((input, entityId) => string.IsNullOrWhiteSpace(input!.UserId), ErrorCodes.UserIdNotSupplied),
                ((input, entityId) => 
                    input!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                    ErrorCodes.EntityDoesNotBelongToUser
                ),
                ((input, entityId) => string.IsNullOrWhiteSpace(input!.Title), ErrorCodes.ChecklistTitleNotFound)
            ],
            [CrudOperation.Delete] = [
                ((input, entityId) => string.IsNullOrWhiteSpace(entityId), ErrorCodes.ChecklistIdNotSupplied)
            ]
        };
    }

    public async Task<Result<IList<ChecklistDTO>, AppError>> GetUserChecklists()
    {
        //A more complete validation can be performed here since we have the unitOfWork and access to all repos
        if (string.IsNullOrWhiteSpace(CrudSettings.UserId))
            return new AppError(ErrorCodes.UserIdNotSupplied);

        var checklists = await _repository.Find(n => n.UserId == CrudSettings.UserId).ConfigureAwait(false);
        List<ChecklistDTO> result = _mapper.Map<List<ChecklistDTO>>(checklists);
        return result;
    }

    public override async Task<Result<(ChecklistDTO Entity, string Id), AppError>> Create(ChecklistDTO input)
    {
        _autoSave = false;
        try {
            await _unitOfWork.StartTransaction().ConfigureAwait(false);
            var insertResult = await base.Create(input).ConfigureAwait(false);

            if (insertResult.Error is not null)
            {
                await _unitOfWork.Rollback().ConfigureAwait(false);
                return insertResult.Error;
            }

            if (input.Details != null && input.Details.Count > 0)
            {
                var flattenedDetails = ApplicationModelsMappingProfile.FlattenAndGenerateChecklistDetails(
                    insertResult.Value.Id, _mapper.Map<List<ChecklistDetail>>(input.Details)
                );
                await InsertDetails(flattenedDetails).ConfigureAwait(false);
            }

            await _unitOfWork.Commit().ConfigureAwait(false);
            input = input with { Id = insertResult.Value!.Id };
            return (input, input.Id);
        }
        catch (Exception e)
        {
            await _unitOfWork.Rollback().ConfigureAwait(false);
            _logger.LogError(e, ErrorMessages.DbTransactionError);
            return new AppError(ErrorCodes.RepoProblem);
        }
        finally {
            _autoSave = true;
        }
    }

    public override async Task<Result<ChecklistDTO, AppError>> Update(string entityId, ChecklistDTO input)
    {
        _autoSave = false;
        var entityResult = await ValidateAndGetEntity(CrudOperation.Update, entityId, input)
            .ConfigureAwait(false);

        if (entityResult.Error != null)
            return entityResult.Error;
        
        var entity = entityResult.Value;
        var allEntityDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(entity!.Details);
        var allEntityDetailsIDs = allEntityDetails.Select(d => d.Id);
        
        var updateDto = input as UpdateChecklistDTO;

        var detailsToDeleteIDsIntersected = allEntityDetailsIDs.Intersect(updateDto?.DetailsToDelete ?? []);
        if (detailsToDeleteIDsIntersected.Count() != (updateDto?.DetailsToDelete ?? []).Count()) // comparing int to int?
            return new AppError(ErrorCodes.OneOrMoreChecklistDetailToDeleteNotFound);
        
        var allDetailsToDelete = ApplicationModelsMappingProfile.FindAllDetailsToRemove(
            allEntityDetails, updateDto?.DetailsToDelete
        );

        var detailsToUpdate = updateDto?.ProcessDetailsToUpdate(allDetailsToDelete);
        var detailsToUpdateIDs = detailsToUpdate?.Select(d => d.Id);
        var detailsToUpdateIDsIntersected = allEntityDetailsIDs.Intersect(detailsToUpdateIDs ?? []);
        if (detailsToUpdateIDsIntersected.Count() != (detailsToUpdateIDs ?? []).Count())
            return new AppError(ErrorCodes.OneOrMoreChecklistDetailToUpdateNotFound);

        var flattenedDetailsToAdd = ApplicationModelsMappingProfile.FlattenAndGenerateChecklistDetails(
            entityId, _mapper.Map<List<ChecklistDetail>>(updateDto!.DetailsToAdd)
        );
        
        var checklist = _mapper.Map<Checklist>(input);
        // Update entity modificationDate
        checklist.Id = entityId;
        checklist.CreationDate = entity.CreationDate;
        checklist.ModificationDate = DateTime.Now;

        var transactionFunc = () => TransactionallyUpdateChecklist(
            checklist, flattenedDetailsToAdd, _mapper.Map<List<ChecklistDetail>>(detailsToUpdate), allDetailsToDelete
        );

        var transactionResult = await _unitOfWork.RunAsyncTransaction(transactionFunc, _logger, ResetAutoSaveOption)
            .ConfigureAwait(false);
        
        if (transactionResult != null)
            return transactionResult;

        return updateDto with {
            Details = [],
            DetailsToAdd = _mapper.Map<List<ChecklistDetailDTO>?>(flattenedDetailsToAdd),
            DetailsToUpdate = detailsToUpdate,
            DetailsToDelete = allDetailsToDelete
        };
    }

    public override async Task<AppError?> DeleteById(string entityId)
    {
        var entityResult = await ValidateAndGetEntity(CrudOperation.Delete, entityId)
            .ConfigureAwait(false);

        if (entityResult.Error != null)
            return entityResult.Error;
        
        _autoSave = false;
        var transactionFunc = () => TransactionallyDeleteChecklist(entityId);
        var transactionResult = await _unitOfWork.RunAsyncTransaction(transactionFunc, _logger, ResetAutoSaveOption)
            .ConfigureAwait(false);

        return transactionResult;
    }

    public async Task TransactionallyDeleteChecklist(string checklistId)
    {
        await _checklistRepo.DeleteDetails(checklistId, autoSave: false).ConfigureAwait(false);
        await _checklistRepo.DeleteById(checklistId, autoSave: false).ConfigureAwait(false);
    }

    public async Task TransactionallyUpdateChecklist(
        Checklist checklist, List<ChecklistDetail>? detailsToAdd,
        List<ChecklistDetail>? detailsToUpdate, List<string>? detailsToDelete
    ){
        await _checklistRepo.Update(checklist, autoSave: false).ConfigureAwait(false);
        await DeleteDetails(detailsToDelete).ConfigureAwait(false);
        await UpdateDetails(detailsToUpdate).ConfigureAwait(false);
        await InsertDetails(detailsToAdd).ConfigureAwait(false);
    }


    private async Task InsertDetails(List<ChecklistDetail>? details)
    {
        if (details?.Count == 0)
            return;

        var insertTasks = new List<Task>();
        details!.ForEach(d => 
            insertTasks.Add(_checklistDetailsRepo.Create(d, autoSave: false)
        ));
        await Task.WhenAll(insertTasks).ConfigureAwait(false);
    }

    private async Task UpdateDetails(List<ChecklistDetail>? details)
    {
        if (details?.Count == 0)
            return;

        // [[[[[ This does work concurrently ]]]]] Update impl does not use anything async but it works (don't know why)
        // var updateTasks = new List<Task>();
        // details.ForEach(d => 
        //     updateTasks.Add(_checklistDetailsRepo.Update(d, autoSave: false)
        // ));
        // await Task.WhenAll(updateTasks).ConfigureAwait(false);
        foreach (var d in details!)
        {
            await _checklistDetailsRepo.Update(d, autoSave: false).ConfigureAwait(false);
        }
    }

    private async Task DeleteDetails(List<string>? detailsIDs)
    {
        if (detailsIDs?.Count == 0)
            return;

        // var deleteTasks = new List<Task>();
        // detailsIDs.ForEach(id => 
        //     deleteTasks.Add(_checklistDetailsRepo.DeleteById(id, autoSave: false)
        // ));
        // [[[[[ This doesn't work concurrently ]]]]] Delete impl does not use anything async
        // await Task.WhenAll(deleteTasks).ConfigureAwait(false);

        // TODO: Deleting a parentDetailId can lead to orphan ChecklistDetails, although this doesn't break anything,
        // only creates orphans
        foreach (var id in detailsIDs!)
        {
            await _checklistDetailsRepo.DeleteById(id, autoSave: false).ConfigureAwait(false);
        }
    }

    public override bool EntityBelongsToUser(Checklist entity) {
        return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
    }
}
