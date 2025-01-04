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

using ValidationFunc = (Func<ChecklistDTO?, string?, bool>, string);

public class ChecklistCrudService : EntityCrudService<Checklist, ChecklistDTO>, IChecklistCrudService
{
    private readonly Dictionary<CrudOperation, List<ValidationFunc>> _validations;

    public override Dictionary<CrudOperation, List<ValidationFunc>>ValidationsByOperation => _validations;

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
            await _unitOfWork.StartTransaction().ConfigureAwait(false);
            var insertResult = await base.Create(input).ConfigureAwait(false);

            if (insertResult.Error is not null)
                return insertResult.Error;

            if (input.Details != null && input.Details.Count > 0)
            {
                var flattenedDetails = FlattenDetails(insertResult.Value.Id, input.Details);
                await InsertDetails(flattenedDetails).ConfigureAwait(false);
            }

            await _unitOfWork.Commit().ConfigureAwait(false);
            input = input with { Id = insertResult.Value!.Id };
            return (input, input.Id);
        }
        catch {
            await _unitOfWork.Rollback().ConfigureAwait(false);
            throw;
        }
    }

    public override async Task<Result<ChecklistDTO, AppError>> Update(string entityId, ChecklistDTO input)
    {
        var k = input as UpdateChecklistDTO;
        return await base.Update(entityId, input).ConfigureAwait(false);
    }


    private async Task InsertDetails(List<ChecklistDetailDTO> details)
    {
        var insertTasks = new List<Task>();
        details.ForEach(d => 
            insertTasks.Add(_repository.Create(_mapper.Map<Checklist>(d))
        ));
        await Task.WhenAll(insertTasks).ConfigureAwait(false);
    }

    private List<ChecklistDetailDTO> FlattenDetails(
        string parentChecklistId, IList<ChecklistDetailDTO> details, string? parentDetailId = null
    ) {
        var flattenedDetails = new List<ChecklistDetailDTO>();
        for(int i = 0; i < details.Count; i++)
        {
            var detail = details[i];
            detail = detail with { 
                Id = Guid.CreateVersion7().ToString("N"),
                ParentDetailId = parentDetailId,
                ChecklistId = parentChecklistId,
                CreationDate = DateTime.Now,
                ModificationDate = null,
            };

            flattenedDetails.Add(detail);

            if (detail.SubItems != null && detail.SubItems.Count > 0)
            {
                // detail.SubItems = FillDetails(parentChecklistId, detail.SubItems, detail.Id);
                flattenedDetails.AddRange(FlattenDetails(parentChecklistId, detail.SubItems, detail.Id));
            }   
                
        }
        return flattenedDetails;
    }

    public override bool EntityBelongsToUser(Checklist entity) {
        return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
    }
}
