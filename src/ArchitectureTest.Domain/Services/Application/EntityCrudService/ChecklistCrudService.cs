using System;
using System.Collections.Generic;
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
                ((input, entityId) => string.IsNullOrWhiteSpace(input!.Title), ErrorCodes.NoteTitleNotFound)
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
                var flattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(
                    insertResult.Value.Id, input.Details
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
        var k = input as UpdateChecklistDTO;
        return await base.Update(entityId, input).ConfigureAwait(false);
    }

    public override async Task<AppError?> DeleteById(string entityId)
    {
        if (RequestIsValid(CrudOperation.Delete, entityId: entityId) is AppError requestError && requestError is not null)
            return requestError;

        var entity = await _repository.GetById(entityId).ConfigureAwait(false);
        if (entity == null)
            return new AppError(ErrorCodes.EntityNotFound);
        
        if (!EntityBelongsToUser(entity))
            return new AppError(ErrorCodes.EntityDoesNotBelongToUser);
        
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

    private async Task InsertDetails(List<ChecklistDetailDTO> details)
    {
        var insertTasks = new List<Task>();
        details.ForEach(d => 
            insertTasks.Add(_checklistDetailsRepo.Create(_mapper.Map<ChecklistDetail>(d), false)
        ));
        await Task.WhenAll(insertTasks).ConfigureAwait(false);
    }

    public override bool EntityBelongsToUser(Checklist entity) {
        return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
    }
}
