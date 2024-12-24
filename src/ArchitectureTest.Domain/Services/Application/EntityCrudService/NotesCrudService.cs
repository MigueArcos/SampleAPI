using ArchitectureTest.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService;

public class NotesCrudService : EntityCrudService<Note>, INotesCrudService {
    public NotesCrudService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    public override Dictionary<CrudOperation, List<(Func<Note?, long?, bool>, string)>> ValidationsByOperation => new() {
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

    public override bool EntityBelongsToUser(Note entity) {
        return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
    }

    public async Task<Result<IList<Note>, AppError>> GetUserNotes() {
        //A more complete validation can be performed here since we have the unitOfWork and access to all repos
        if (CrudSettings.UserId < 1) return new AppError(ErrorCodes.UserIdNotSupplied);
        var notes = await _repository.Find(n => n.UserId == CrudSettings.UserId).ConfigureAwait(false);

        // TODO: Check why these lines doesn't work even though the signature of the method expects an IList
        // IList<Note> result = notes.ToList();
        // or
        // (<Result<IList<Note>, AppError>>) notes;
        // (notes) as Result<IList<Note>, AppError>;
        List<Note> result = notes.ToList();
        return result;
    }
}
