using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using ArchitectureTest.Domain.Models.Converters;
using ArchitectureTest.Domain.Models.StatusCodes;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Models;

namespace ArchitectureTest.Domain.ServiceLayer.EntityCrudService {
	public class EntityCrudSettings {
		public bool ValidateEntityBelongsToUser { get; set; }
		public long UserId { get; set; }
	}
	public abstract class EntityCrudService<TEntity, TDto> : IDtoConverter<TEntity, TDto> where TEntity : class where TDto : BasicDTO, IEntityConverter<TEntity> {
		protected readonly IRepository<TEntity> repository;
		protected readonly IUnitOfWork unitOfWork;
		public EntityCrudSettings CrudSettings { get; set; } = new EntityCrudSettings {
			ValidateEntityBelongsToUser = false
		};
		public EntityCrudService(IUnitOfWork unitOfWork) {
			this.repository = unitOfWork.Repository<TEntity>();
			this.unitOfWork = unitOfWork;
		}
		public virtual async Task<TDto> Post(TDto dto) {
			if (RequestIsValid(RequestType.Post, dto: dto)) {
				var entity = await repository.Post(dto.ToEntity());
				return ToDTO(entity);
			}
			throw ErrorStatusCode.UnknownError;
		}

		public virtual async Task<TDto> GetById(long entityId) {
			if (RequestIsValid(RequestType.Get, entityId: entityId)) {
				var entity = await repository.GetById(entityId);
				if (entity != null) {
					if (!EntityBelongsToUser(entity)) throw ErrorStatusCode.EntityDoesNotBelongToUser;
					return ToDTO(entity);
				}
				throw ErrorStatusCode.EntityNotFound;
			}
			throw ErrorStatusCode.UnknownError;
		}

		public virtual async Task<TDto> Put(long entityId, TDto dto) {
			if (RequestIsValid(RequestType.Put, entityId: entityId, dto: dto)) {
				var entity = await repository.GetById(entityId);
				if (entity != null && !EntityBelongsToUser(entity)) throw ErrorStatusCode.EntityDoesNotBelongToUser;
				dto.Id = entityId;
				var result = await repository.Put(dto.ToEntity());
				if (result) return dto;
				else throw ErrorStatusCode.RepoProblem;
			}
			throw ErrorStatusCode.UnknownError;
		}

		public virtual async Task<bool> Delete(long entityId) {
			if (RequestIsValid(RequestType.Delete, entityId: entityId)) {
				var entity = await repository.GetById(entityId);
				if (entity != null && !EntityBelongsToUser(entity)) throw ErrorStatusCode.EntityDoesNotBelongToUser;
				var result = await repository.DeleteById(entityId);
				return result;
			}
			throw ErrorStatusCode.UnknownError;
		}

		public abstract bool RequestIsValid(RequestType requestType, long? entityId = null, TDto dto = null);
		public abstract bool EntityBelongsToUser(TEntity entity);
		public abstract TDto ToDTO(TEntity entity);
		public abstract IList<TDto> ToDTOs(IList<TEntity> entities);
	}
}
