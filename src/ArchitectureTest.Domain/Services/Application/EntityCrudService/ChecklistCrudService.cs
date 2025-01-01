using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService;

public class ChecklistCrudService : EntityCrudService<Checklist>, IChecklistCrudService
{
    private readonly Dictionary<CrudOperation, List<(Func<Checklist?, string?, bool>, string)>> _validations;
    public override Dictionary<CrudOperation, List<(Func<Checklist?, string?, bool>, string)>> ValidationsByOperation => _validations;

    public ChecklistCrudService(IUnitOfWork unitOfWork) : base(unitOfWork) {
        _validations = new (){
            [CrudOperation.Create] = [
                ((entity, entityId) => entity is null, ErrorCodes.InputDataNotFound),
                ((entity, entityId) => string.IsNullOrWhiteSpace(entity!.UserId), ErrorCodes.UserIdNotSupplied),
                ((entity, entityId) => 
                    entity!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                    ErrorCodes.CannotCreateDataForThisUserId
                ),
                ((entity, entityId) => string.IsNullOrWhiteSpace(entity!.Title), ErrorCodes.NoteTitleNotFound),
            ],
            [CrudOperation.ReadById] = [
                ((entity, entityId) => string.IsNullOrWhiteSpace(entityId), ErrorCodes.ChecklistIdNotSupplied)
            ],
            [CrudOperation.Update] = [
                ((entity, entityId) => entityId == null, ErrorCodes.ChecklistIdNotSupplied),
                ((entity, entityId) => entity is null, ErrorCodes.InputDataNotFound),
                ((entity, entityId) => string.IsNullOrWhiteSpace(entity!.UserId), ErrorCodes.UserIdNotSupplied),
                ((entity, entityId) => 
                    entity!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                    ErrorCodes.EntityDoesNotBelongToUser
                ),
                ((entity, entityId) => string.IsNullOrWhiteSpace(entity!.Title), ErrorCodes.NoteTitleNotFound)
            ],
            [CrudOperation.Delete] = [
                ((entity, entityId) => string.IsNullOrWhiteSpace(entityId), ErrorCodes.ChecklistIdNotSupplied)
            ]
        };
    }

    public async Task<Result<IList<Checklist>, AppError>> GetUserChecklists()
    {
        //A more complete validation can be performed here since we have the unitOfWork and access to all repos
        if (string.IsNullOrWhiteSpace(CrudSettings.UserId))
            return new AppError(ErrorCodes.UserIdNotSupplied);

        var checklists = await _repository.Find(n => n.UserId == CrudSettings.UserId).ConfigureAwait(false);
        List<Checklist> result = checklists.ToList();
        return result;
    }

    public override async Task<Result<Checklist, AppError>> Create(Checklist inputEntity)
    {
        try {
            await _unitOfWork.StartTransaction();
            var insertResult = await base.Create(inputEntity).ConfigureAwait(false);

            if (insertResult.Error is not null)
                return insertResult.Error;

            if (inputEntity.Details != null && inputEntity.Details.Count > 0)
                await PostDetails(insertResult.Value!.Id, inputEntity.Details).ConfigureAwait(false);

            await _unitOfWork.Commit();
            inputEntity.Id = insertResult.Value!.Id;
            return inputEntity;
        }
        catch {
            await _unitOfWork.Rollback();
            throw;
        }
    }

    private IList<ChecklistDetail> GetChecklistDetails(ICollection<ChecklistDetail> details, string? parentDetailId = null)
    {
        var selection = details.Where(d => d.ParentDetailId == parentDetailId).Select(cD => new ChecklistDetail {
            Id = cD.Id,
            ChecklistId = cD.ChecklistId,
            ParentDetailId = cD.ParentDetailId,
            TaskName = cD.TaskName,
            Status = cD.Status,
            CreationDate = cD.CreationDate,
            ModificationDate = cD.ModificationDate
        }).ToList();
        selection.ForEach(i => {
            i.SubItems = GetChecklistDetails(details, i.Id);
        });
        return selection;
    }

    // TODO: Optimize this method now that we have the logic generation in app rather than in DB
    private async Task<bool> PostDetails(
        string parentChecklistId, IList<ChecklistDetail> details, string? parentDetailId = null
    ) {
        for(int i = 0; i < details.Count; i++){
            var d = details[i];
            d.ChecklistId = parentChecklistId;
            d.ParentDetailId = parentDetailId;
            d.Id = Guid.CreateVersion7().ToString("N");
            await _unitOfWork.Repository<ChecklistDetail>().Create(d)
                .ConfigureAwait(false);

            if (d.SubItems != null && d.SubItems.Count > 0) {    
                await PostDetails(parentChecklistId, d.SubItems, d.Id).ConfigureAwait(false);
            }
        }
        return true;
    }

    public override bool EntityBelongsToUser(Checklist entity) {
        return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
    }

}
