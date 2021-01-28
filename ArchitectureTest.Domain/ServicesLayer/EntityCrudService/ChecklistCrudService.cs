using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.StatusCodes;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureTest.Domain.ServicesLayer.EntityCrudService.Contracts;

namespace ArchitectureTest.Domain.ServiceLayer.EntityCrudService {
	public class ChecklistCrudService : EntityCrudService<Checklist, ChecklistDTO>, IChecklistCrudService {
		public ChecklistCrudService(IUnitOfWork unitOfWork) : base(unitOfWork) { }
		public override bool RequestIsValid(RequestType requestType, long? entityId = null, ChecklistDTO dto = null) {
			switch (requestType) {
				case RequestType.Post:
					if (dto.UserId < 1) throw ErrorStatusCode.UserIdNotSupplied;
					if (string.IsNullOrWhiteSpace(dto.Title)) throw ErrorStatusCode.NoteTitleNotFound;
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
					if (entityId < 0) throw ErrorStatusCode.NoteIdNotSupplied;
					break;
			}
			return true;
		}
		public async Task<IList<ChecklistDTO>> GetUserChecklists() {
			//A more complete validation can be performed here since we have the unitOfWork and access to all repos
			if (CrudSettings.UserId < 1) throw ErrorStatusCode.UserIdNotSupplied;
			var notes = await repository.Get(n => n.UserId == CrudSettings.UserId);
			return ToDTOs(notes);
		}

		public override async Task<ChecklistDTO> Post(ChecklistDTO dto) {
			try {
				unitOfWork.StartTransaction();
				var insertResult = await base.Post(dto);
				if (dto.Details != null && dto.Details.Count > 0) await PostDetails(insertResult.Id ?? 0, dto.Details);
				unitOfWork.Commit();
				dto.Id = insertResult.Id;
				return dto;
			}
			catch (Exception exception) {
				unitOfWork.Rollback();
				//We should never expose real exceptions, so we will catch all unknown exceptions (DatabaseErrors, Null Errors, Index errors, etc...) and rethrow an UnknownError after log
				Console.WriteLine(exception);
				throw ErrorStatusCode.UnknownError;
			}
		}
		public override ChecklistDTO ToDTO(Checklist entity) {
			return new ChecklistDTO {
				Id = entity.Id,
				Title = entity.Title,
				UserId = entity.UserId,
				CreationDate = entity.CreationDate ?? new System.DateTime(default),
				ModificationDate = entity.ModificationDate ?? new System.DateTime(default),
				Details = GetChecklistDetails(entity.ChecklistDetail)
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
				ChecklistDetail checklistDetailEntity = await unitOfWork.Repository<ChecklistDetail>().Post(d.ToEntity());
				if (d.SubItems != null && d.SubItems.Count > 0) {	
					await PostDetails(parentChecklistId, d.SubItems, checklistDetailEntity.Id);
				}
			}
			return true;
		}

		public override bool EntityBelongsToUser(Checklist entity) {
			return !CrudSettings.ValidateEntityBelongsToUser || entity.UserId == CrudSettings.UserId;
		}
	}
}
