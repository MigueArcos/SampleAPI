using ArchitectureTest.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.NewImpl.Contracts;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.NewImpl;

public class NotesCrudService : EntityCrudService<NoteEntity>, INotesCrudService {
    public NotesCrudService(IDomainUnitOfWork unitOfWork) : base(unitOfWork) { }

    public override Dictionary<CrudOperation, List<(Func<NoteEntity?, long?, bool>, string)>> ValidationsByOperation => new() {
        [CrudOperation.Create] = [
            ((dto, entityId) => dto is null, ErrorCodes.InputDataNotFound),
            ((dto, entityId) => dto!.UserId < 1, ErrorCodes.UserIdNotSupplied),
            ((dto, entityId) => 
                dto!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                ErrorCodes.CannotCreateDataForThisUserId
            ),
            ((dto, entityId) => string.IsNullOrWhiteSpace(dto!.Title), ErrorCodes.NoteTitleNotFound),
        ],
        [CrudOperation.ReadById] = [
            ((dto, entityId) => entityId < 1, ErrorCodes.NoteIdNotSupplied)
        ],
        [CrudOperation.Update] = [
            ((dto, entityId) => entityId == null, ErrorCodes.NoteIdNotSupplied),
            ((dto, entityId) => dto is null, ErrorCodes.InputDataNotFound),
            ((dto, entityId) => dto!.UserId < 1, ErrorCodes.UserIdNotSupplied),
            ((dto, entityId) => 
                dto!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                ErrorCodes.EntityDoesNotBelongToUser
            ),
            ((dto, entityId) => string.IsNullOrWhiteSpace(dto!.Title), ErrorCodes.NoteTitleNotFound)
        ],
        [CrudOperation.Delete] = [
            ((dto, entityId) => entityId < 1, ErrorCodes.NoteIdNotSupplied)
        ]
    };

    public override void AggregateData(NoteEntity entity)
    {
        
    }

    public override bool EntityBelongsToUser(NoteEntity entity) {
        return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
    }

    public async Task<Result<IList<NoteEntity>, AppError>> GetUserNotes() {
        //A more complete validation can be performed here since we have the unitOfWork and access to all repos
        if (CrudSettings.UserId < 1) return new AppError(ErrorCodes.UserIdNotSupplied);
        var notes = await _repository.Find(n => n.UserId == CrudSettings.UserId && n.CreationDate > new DateTime(2024, 12, 10)/* does not work && n.Content == "sdg"*/).ConfigureAwait(false);

        // TODO: Check why these lines doesn't work even though toDTOs returns an IList
        // IList<NoteDTO> result = ToDTOs(notes).ToList();
        // or
        // (<Result<IList<NoteDTO>, AppError>>) ToDTOs(notes);
        // ToDTOs(notes) as Result<IList<NoteDTO>, AppError>;
        List<NoteEntity> result = notes.ToList();
        return result;
    }
}
