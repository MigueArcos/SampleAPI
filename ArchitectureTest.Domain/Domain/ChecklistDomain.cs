using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.Repositories.BasicRepo;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Domain.UnitOfWork;
using ArchitectureTest.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Domain {
	public class ChecklistDomain : BaseDomain<Checklist, ChecklistDTO> {
		public ChecklistDomain(IRepository<Checklist> repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork) { }
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
		public override async Task<ChecklistDTO> Post(ChecklistDTO dto) {
			try {
				unitOfWork.StartTransaction();
				var insertResult = await base.Post(dto);
				var tasks = new List<Task>();
				dto.Details.ForEach(d => {
					d.ChecklistId = insertResult.Id ?? 0;
					tasks.Add(unitOfWork.ChecklistDetailRepository.Post(d.ToEntity()));
				});
				await Task.WhenAll(tasks);
				unitOfWork.Commit();
				dto.Id = insertResult.Id;
				return dto;
			}
			catch (Exception exception) {
				unitOfWork.Rollback();
				throw DefaultCatchHandler(exception);
			}
		}
		public override ChecklistDTO ToDTO(Checklist entity) {
			return new ChecklistDTO {
				Id = entity.Id,
				Title = entity.Title,
				UserId = entity.UserId,
				CreationDate = entity.CreationDate ?? new System.DateTime(default),
				ModificationDate = entity.ModificationDate ?? new System.DateTime(default)
			};
		}

		public override IList<ChecklistDTO> ToDTOs(IList<Checklist> entities) {
			return entities.Select(n => ToDTO(n)).ToList();
		}
	}
}
