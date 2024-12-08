using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.StatusCodes;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureTest.Domain.ServicesLayer.EntityCrudService.Contracts;

namespace ArchitectureTest.Domain.ServiceLayer.EntityCrudService;

public class NotesCrudService : EntityCrudService<Note, NoteDTO>, INotesCrudService {
	public NotesCrudService(IUnitOfWork unitOfWork) : base(unitOfWork) { }
	public override bool RequestIsValid(RequestType requestType, long? entityId = null, NoteDTO dto = null) {
		switch (requestType) {
			case RequestType.Post:
				if (dto.UserId < 1) throw ErrorStatusCode.UserIdNotSupplied;
				if (string.IsNullOrWhiteSpace(dto.Title)) throw ErrorStatusCode.NoteTitleNotFound;
				if (string.IsNullOrEmpty(dto.Content)) dto.Content = string.Empty;
				break;
			case RequestType.Get:
				if (entityId < 1) throw ErrorStatusCode.NoteIdNotSupplied;
				break;
			case RequestType.Put:
				if (entityId == null) throw ErrorStatusCode.NoteIdNotSupplied;
				if (dto.UserId < 1) throw ErrorStatusCode.UserIdNotSupplied;
				if (string.IsNullOrWhiteSpace(dto.Title)) throw ErrorStatusCode.NoteTitleNotFound;
				break;
			case RequestType.Delete:
				if (entityId < 1) throw ErrorStatusCode.NoteIdNotSupplied;
				break;
		}
		return true;
	}
	
	public override NoteDTO ToDTO(Note entity) {
		return new NoteDTO {
			Title = entity.Title,
			Content = entity.Content,
			UserId = entity.UserId,
			CreationDate = entity.CreationDate ?? new System.DateTime(default),
			ModificationDate = entity.ModificationDate ?? new System.DateTime(default)
		};
	}

	public override IList<NoteDTO> ToDTOs(IList<Note> entities) {
		return entities.Select(n => ToDTO(n)).ToList();
	}

	public override bool EntityBelongsToUser(Note entity) {
		return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
	}

    public async Task<IList<NoteDTO>> GetUserNotes() {
        //A more complete validation can be performed here since we have the unitOfWork and access to all repos
        if (CrudSettings.UserId < 1) throw ErrorStatusCode.UserIdNotSupplied;
        var notes = await repository.Get(n => n.UserId == CrudSettings.UserId);
        return ToDTOs(notes);
    }
}
