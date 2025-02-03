using ArchitectureTest.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using AutoMapper;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService;

using ValidationFunc = (Func<NoteDTO?, string?, bool>, string);


public class NotesCrudService : EntityCrudService<Note, NoteDTO>, INotesCrudService
{
    private readonly Dictionary<CrudOperation, List<ValidationFunc>> _validations;
    public override Dictionary<CrudOperation, List<ValidationFunc>> ValidationsByOperation => _validations;

    public NotesCrudService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
        _validations = new() {
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
                ((input, entityId) => string.IsNullOrWhiteSpace(entityId), ErrorCodes.NoteIdNotSupplied)
            ],
            [CrudOperation.Update] = [
                ((input, entityId) => entityId == null, ErrorCodes.NoteIdNotSupplied),
                ((input, entityId) => input is null, ErrorCodes.InputDataNotFound),
                ((input, entityId) => string.IsNullOrWhiteSpace(input!.UserId), ErrorCodes.UserIdNotSupplied),
                ((input, entityId) => 
                    input!.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser,
                    ErrorCodes.EntityDoesNotBelongToUser
                ),
                ((input, entityId) => string.IsNullOrWhiteSpace(input!.Title), ErrorCodes.NoteTitleNotFound)
            ],
            [CrudOperation.Delete] = [
                ((input, entityId) => string.IsNullOrWhiteSpace(entityId), ErrorCodes.NoteIdNotSupplied)
            ]
        };
    }

    public override bool EntityBelongsToUser(Note entity) {
        return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
    }

    public async Task<Result<IList<NoteDTO>, AppError>> GetUserNotes()
    {
        // A more complete validation can be performed here since we have the unitOfWork and access to all repos
        if (string.IsNullOrWhiteSpace(CrudSettings.UserId))
            return new AppError(ErrorCodes.UserIdNotSupplied);

        var notes = await _repository.Find(n => n.UserId == CrudSettings.UserId).ConfigureAwait(false);

        // TODO: Check why these lines doesn't work even though the signature of the method expects an IList
        // IList<Note> result = notes.ToList();
        // or
        // (<Result<IList<Note>, AppError>>) notes;
        // (notes) as Result<IList<Note>, AppError>;
        List<NoteDTO> result = _mapper.Map<List<NoteDTO>>(notes);
        return result;
    }
}
