using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureTest.Domain.ServicesLayer.EntityCrudService.Contracts;
using ArchitectureTest.Domain.Models.Enums;

namespace ArchitectureTest.Domain.ServiceLayer.EntityCrudService;

public class ChecklistCrudService : EntityCrudService<Checklist, ChecklistDTO>, IChecklistCrudService {
    public ChecklistCrudService(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    public override AppError? RequestIsValid(RequestType requestType, long? entityId = null, ChecklistDTO? dto = null) {
        switch (requestType) {
            case RequestType.Post:
                if (dto is null)
                    return new AppError(ErrorCodes.InputDataNotFound);

                if (dto.UserId < 1)
                    return new AppError(ErrorCodes.UserIdNotSupplied);
                
                if (dto.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser)
                    return new AppError(ErrorCodes.CannotCreateDataForThisUserId);

                if (string.IsNullOrWhiteSpace(dto.Title))
                    return new AppError(ErrorCodes.NoteTitleNotFound);
                break;
            case RequestType.Get:
                if (entityId < 1)
                    return new AppError(ErrorCodes.ChecklistIdNotSupplied);
                break;
            case RequestType.Put:
                if (entityId == null)
                    return new AppError(ErrorCodes.ChecklistIdNotSupplied);
                
                if (dto is null)
                    return new AppError(ErrorCodes.InputDataNotFound);

                if (dto.UserId < 1)
                    return new AppError(ErrorCodes.UserIdNotSupplied);
                
                if (dto.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser)
                    return new AppError(ErrorCodes.CannotCreateDataForThisUserId);

                if (string.IsNullOrWhiteSpace(dto.Title))
                    return new AppError(ErrorCodes.NoteTitleNotFound);
                break;
            case RequestType.Delete:
                if (entityId < 0)
                    return new AppError(ErrorCodes.ChecklistIdNotSupplied);
                break;
        }
        return null;
    }
    public async Task<Result<IList<ChecklistDTO>, AppError>> GetUserChecklists() {
        //A more complete validation can be performed here since we have the unitOfWork and access to all repos
        if (CrudSettings.UserId < 1)
            return new AppError(ErrorCodes.UserIdNotSupplied);

        var checklists = await _repository.Find(n => n.UserId == CrudSettings.UserId).ConfigureAwait(false);
        List<ChecklistDTO> result = ToDTOs(checklists).ToList();
        return result;
    }

    public override async Task<Result<ChecklistDTO, AppError>> Add(ChecklistDTO dto) {
        try {
            _unitOfWork.StartTransaction();
            var insertResult = await base.Add(dto).ConfigureAwait(false);

            if (insertResult.Error is not null)
                return insertResult.Error;

            if (dto.Details != null && dto.Details.Count > 0)
                await PostDetails(insertResult.Value!.Id, dto.Details).ConfigureAwait(false);

            _unitOfWork.Commit();
            dto.Id = insertResult.Value!.Id;
            return dto;
        }
        catch {
            _unitOfWork.Rollback();
            throw;
        }
    }

    public override ChecklistDTO ToDTO(Checklist entity) {
        return new ChecklistDTO {
            Id = entity.Id,
            Title = entity.Title,
            UserId = entity.UserId,
            CreationDate = entity.CreationDate ?? new System.DateTime(default),
            ModificationDate = entity.ModificationDate ?? new System.DateTime(default),
            Details = GetChecklistDetails(entity.ChecklistDetails)
        };
    }

    public override IList<ChecklistDTO> ToDTOs(IList<Checklist> entities) {
        return entities.Select(n => ToDTO(n)).ToList();
    }

    private IList<ChecklistDetailDTO> GetChecklistDetails(ICollection<ChecklistDetail> details, long? parentDetailId = null){
        var selection = details.Where(d => d.ParentDetailId == parentDetailId).Select(cD => new ChecklistDetailDTO {
            Id = cD.Id,
            ChecklistId = cD.ChecklistId,
            ParentDetailId = cD.ParentDetailId,
            TaskName = cD.TaskName,
            Status = cD.Status,
            CreationDate = cD.CreationDate ?? new System.DateTime(default),
            ModificationDate = cD.ModificationDate ?? new System.DateTime(default)
        }).ToList();
        selection.ForEach(i => {
            i.SubItems = GetChecklistDetails(details, i.Id);
        });
        return selection;
    }

    private async Task<bool> PostDetails(long parentChecklistId, IList<ChecklistDetailDTO> details, long? parentDetailId = null) {
        for(int i = 0; i < details.Count; i++){
            var d = details[i];
            d.ChecklistId = parentChecklistId;
            d.ParentDetailId = parentDetailId;
            ChecklistDetail checklistDetailEntity = await _unitOfWork.Repository<ChecklistDetail>().Add(d.ToEntity())
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
