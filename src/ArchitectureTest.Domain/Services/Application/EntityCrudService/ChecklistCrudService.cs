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

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService;

public class ChecklistCrudService : EntityCrudService<Checklist, ChecklistDTO>, IChecklistCrudService
{
    private readonly Dictionary<CrudOperation, List<(Func<ChecklistDTO?, string?, bool>, string)>> _validations;

    public override Dictionary<CrudOperation, List<(Func<ChecklistDTO?, string?, bool>, string)>>ValidationsByOperation =>
        _validations;

    public ChecklistCrudService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) {
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
        try {
            await _unitOfWork.StartTransaction();
            var insertResult = await base.Create(input).ConfigureAwait(false);

            if (insertResult.Error is not null)
                return insertResult.Error;

            if (input.Details != null && input.Details.Count > 0)
                await PostDetails(insertResult.Value!.Id!, input.Details).ConfigureAwait(false);

            await _unitOfWork.Commit();
            input = input with { Id = insertResult.Value!.Id };
            return (input, input.Id);
        }
        catch {
            await _unitOfWork.Rollback();
            throw;
        }
    }

    public override async Task<Result<ChecklistDTO, AppError>> Update(string entityId, ChecklistDTO input)
    {
        var k = input as UpdateChecklistDTO;
        return await base.Update(entityId, input);
    }

    private IList<ChecklistDetailDTO> GetChecklistDetails(ICollection<ChecklistDetailDTO> details, string? parentDetailId = null)
    {
        var selection = details.Where(d => d.ParentDetailId == parentDetailId).Select(cD => new ChecklistDetailDTO {
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
        string parentChecklistId, IList<ChecklistDetailDTO> details, string? parentDetailId = null
    ) {
        for(int i = 0; i < details.Count; i++){
            var d = details[i];
            d = d with { 
                Id = Guid.CreateVersion7().ToString("N"),
                ParentDetailId = parentDetailId,
                ChecklistId = parentChecklistId
            };
            await _unitOfWork.Repository<ChecklistDetail>().Create(_mapper.Map<ChecklistDetail>(d))
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
