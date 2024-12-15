using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureTest.Domain.ServicesLayer.EntityCrudService.Contracts;
using System;
using ArchitectureTest.Domain.Models.Enums;

namespace ArchitectureTest.Domain.ServiceLayer.EntityCrudService;

public class NotesCrudService : EntityCrudService<Note, NoteDTO>, INotesCrudService {
	public NotesCrudService(IUnitOfWork unitOfWork) : base(unitOfWork) { }
	public override AppError? RequestIsValid(RequestType requestType, long? entityId = null, NoteDTO? dto = null) {
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

				if (string.IsNullOrEmpty(dto.Content))
					dto.Content = string.Empty;
				break;
			case RequestType.Get:
				if (entityId < 1)
					return new AppError(ErrorCodes.NoteIdNotSupplied);
				break;
			case RequestType.Put:
				if (entityId == null)
					return new AppError(ErrorCodes.NoteIdNotSupplied);

				if (dto is null)
					return new AppError(ErrorCodes.InputDataNotFound);

				if (dto.UserId < 1)
					return new AppError(ErrorCodes.UserIdNotSupplied);
				
				if (dto.UserId != CrudSettings.UserId && CrudSettings.ValidateEntityBelongsToUser)
					return new AppError(ErrorCodes.EntityDoesNotBelongToUser);

				if (string.IsNullOrWhiteSpace(dto.Title))
					return new AppError(ErrorCodes.NoteTitleNotFound);
				break;
			case RequestType.Delete:
				if (entityId < 1)
					return new AppError(ErrorCodes.NoteIdNotSupplied);
				break;
		}
		return null;
	}
	
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
