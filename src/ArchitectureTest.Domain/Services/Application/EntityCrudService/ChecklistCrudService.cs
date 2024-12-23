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

public class ChecklistCrudService : EntityCrudService<Checklist>, IChecklistCrudService {
    public ChecklistCrudService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    public override Dictionary<CrudOperation, List<(Func<Checklist?, long?, bool>, string)>> ValidationsByOperation => new (){
        [CrudOperation.Create] = [
            ((entity, entityId) => entity is null, ErrorCodes.InputDataNotFound),
            ((entity, entityId) => entity!.UserId < 1, ErrorCodes.UserIdNotSupplied),
            ((entity, entityId) => 
                entity!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                ErrorCodes.CannotCreateDataForThisUserId
            ),
            ((entity, entityId) => string.IsNullOrWhiteSpace(entity!.Title), ErrorCodes.NoteTitleNotFound),
        ],
        [CrudOperation.ReadById] = [
            ((entity, entityId) => entityId < 1, ErrorCodes.ChecklistIdNotSupplied)
        ],
        [CrudOperation.Update] = [
            ((entity, entityId) => entityId == null, ErrorCodes.ChecklistIdNotSupplied),
            ((entity, entityId) => entity is null, ErrorCodes.InputDataNotFound),
            ((entity, entityId) => entity!.UserId < 1, ErrorCodes.UserIdNotSupplied),
            ((entity, entityId) => 
                entity!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                ErrorCodes.EntityDoesNotBelongToUser
            ),
            ((entity, entityId) => string.IsNullOrWhiteSpace(entity!.Title), ErrorCodes.NoteTitleNotFound)
        ],
        [CrudOperation.Delete] = [
            ((entity, entityId) => entityId < 1, ErrorCodes.ChecklistIdNotSupplied)
        ]
    };

    public async Task<Result<IList<Checklist>, AppError>> GetUserChecklists() {
        //A more complete validation can be performed here since we have the unitOfWork and access to all repos
        if (CrudSettings.UserId < 1)
            return new AppError(ErrorCodes.UserIdNotSupplied);

        var checklists = await _repository.Find(n => n.UserId == CrudSettings.UserId).ConfigureAwait(false);
        List<Checklist> result = checklists.ToList();
        return result;
    }

    public override async Task<Result<Checklist, AppError>> Add(Checklist inputEntity) {
        try {
            _unitOfWork.StartTransaction();
            var insertResult = await base.Add(inputEntity).ConfigureAwait(false);

            if (insertResult.Error is not null)
                return insertResult.Error;

            if (inputEntity.Details != null && inputEntity.Details.Count > 0)
                await PostDetails(insertResult.Value!.Id, inputEntity.Details).ConfigureAwait(false);

            _unitOfWork.Commit();
            inputEntity.Id = insertResult.Value!.Id;
            return inputEntity;
        }
        catch {
            _unitOfWork.Rollback();
            throw;
        }
    }

    private IList<ChecklistDetail> GetChecklistDetails(ICollection<ChecklistDetail> details, long? parentDetailId = null){
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

    private async Task<bool> PostDetails(
        long parentChecklistId, IList<ChecklistDetail> details, long? parentDetailId = null
    ) {
        for(int i = 0; i < details.Count; i++){
            var d = details[i];
            d.ChecklistId = parentChecklistId;
            d.ParentDetailId = parentDetailId;
            var checklistDetailEntity = await _unitOfWork.Repository<ChecklistDetail>().Add(d)
                .ConfigureAwait(false);

            if (d.SubItems != null && d.SubItems.Count > 0) {    
                await PostDetails(parentChecklistId, d.SubItems, checklistDetailEntity.Id).ConfigureAwait(false);
            }
        }
        return true;
    }

    public override bool EntityBelongsToUser(Checklist entity) {
        return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
    }

}
