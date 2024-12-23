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
            ((entity, entityId) => entity is null, ErrorCodes.InputDataNotFound),
            ((entity, entityId) => entity!.UserId < 1, ErrorCodes.UserIdNotSupplied),
            ((entity, entityId) => 
                entity!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                ErrorCodes.CannotCreateDataForThisUserId
            ),
            ((entity, entityId) => string.IsNullOrWhiteSpace(entity!.Title), ErrorCodes.NoteTitleNotFound),
        ],
        [CrudOperation.ReadById] = [
            ((entity, entityId) => entityId < 1, ErrorCodes.NoteIdNotSupplied)
        ],
        [CrudOperation.Update] = [
            ((entity, entityId) => entityId == null, ErrorCodes.NoteIdNotSupplied),
            ((entity, entityId) => entity is null, ErrorCodes.InputDataNotFound),
            ((entity, entityId) => entity!.UserId < 1, ErrorCodes.UserIdNotSupplied),
            ((entity, entityId) => 
                entity!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                ErrorCodes.EntityDoesNotBelongToUser
            ),
            ((entity, entityId) => string.IsNullOrWhiteSpace(entity!.Title), ErrorCodes.NoteTitleNotFound)
        ],
        [CrudOperation.Delete] = [
            ((entity, entityId) => entityId < 1, ErrorCodes.NoteIdNotSupplied)
        ]
    };

    public override bool EntityBelongsToUser(NoteEntity entity) {
        return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
    }

    public async Task<Result<IList<NoteEntity>, AppError>> GetUserNotes() {
        //A more complete validation can be performed here since we have the unitOfWork and access to all repos
        if (CrudSettings.UserId < 1) return new AppError(ErrorCodes.UserIdNotSupplied);
        // TODO: Expression with variables don't work, check why
        // e.g ...Where(n => n.UserId == CrudSettings.UserId && n.CreationDate > new DateTime(2024, 12, 10))
        var notes = await _repository.Find(n => n.UserId == CrudSettings.UserId).ConfigureAwait(false);

        // TODO: Check why these lines doesn't work even though toentitys returns an IList
        // IList<Noteentity> result = Toentitys(notes).ToList();
        // or
        // (<Result<IList<Noteentity>, AppError>>) Toentitys(notes);
        // Toentitys(notes) as Result<IList<Noteentity>, AppError>;
        List<NoteEntity> result = notes.ToList();
        return result;
    }
}
