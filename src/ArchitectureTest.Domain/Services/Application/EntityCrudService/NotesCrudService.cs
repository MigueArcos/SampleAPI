using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using ArchitectureTest.Domain.Enums;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService;

public class NotesCrudService : EntityCrudService<Note, NoteDTO>, INotesCrudService {
    public NotesCrudService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    public override Dictionary<CrudOperation, List<(Func<NoteDTO?, long?, bool>, string)>> ValidationsByOperation => new() {
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

    public override NoteDTO ToDTO(Note entity) {
        return new NoteDTO {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            UserId = entity.UserId,
            CreationDate = entity.CreationDate ?? new DateTime(default),
            ModificationDate = entity.ModificationDate ?? new DateTime(default)
        };
    }

    public override IList<NoteDTO> ToDTOs(IList<Note> entities) {
        return entities.Select(n => ToDTO(n)).ToList();
    }

    public override bool EntityBelongsToUser(Note entity) {
        return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
    }

    public async Task<Result<IList<NoteDTO>, AppError>> GetUserNotes() {
        //A more complete validation can be performed here since we have the unitOfWork and access to all repos
        if (CrudSettings.UserId < 1) return new AppError(ErrorCodes.UserIdNotSupplied);
        var notes = await _repository.Find(n => n.UserId == CrudSettings.UserId).ConfigureAwait(false);

        // TODO: Check why these lines doesn't work even though toDTOs returns an IList
        // IList<NoteDTO> result = ToDTOs(notes).ToList();
        // or
        // (<Result<IList<NoteDTO>, AppError>>) ToDTOs(notes);
        // ToDTOs(notes) as Result<IList<NoteDTO>, AppError>;
        List<NoteDTO> result = ToDTOs(notes).ToList();
        return result;
    }
}
