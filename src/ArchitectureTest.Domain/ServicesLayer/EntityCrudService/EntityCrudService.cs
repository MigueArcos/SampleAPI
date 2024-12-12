using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using ArchitectureTest.Domain.Models.Converters;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService.Contracts;
using System;
using ArchitectureTest.Domain.Models.Enums;

namespace ArchitectureTest.Domain.ServiceLayer.EntityCrudService;

public class EntityCrudSettings {
	public bool ValidateEntityBelongsToUser { get; set; }
	public long UserId { get; set; }
}

public abstract class EntityCrudService<TEntity, TDto> : ICrudService<TEntity, TDto> 
	where TEntity : class
	where TDto : BasicDTO, IEntityConverter<TEntity> 
{
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
		throw new Exception(ErrorCodes.UnknownError);
	}

	public virtual async Task<TDto> GetById(long entityId) {
		if (RequestIsValid(RequestType.Get, entityId: entityId)) {
			var entity = await repository.GetById(entityId);
			if (entity != null) {
				if (!EntityBelongsToUser(entity)) throw new Exception(ErrorCodes.EntityDoesNotBelongToUser);
				return ToDTO(entity);
			}
			throw new Exception(ErrorCodes.EntityNotFound);
		}
		throw new Exception(ErrorCodes.UnknownError);
	}

	public virtual async Task<TDto> Put(long entityId, TDto dto) {
		if (RequestIsValid(RequestType.Put, entityId: entityId, dto: dto)) {
			var entity = await repository.GetById(entityId);
			if (entity != null) {
				if (!EntityBelongsToUser(entity)) throw new Exception(ErrorCodes.EntityDoesNotBelongToUser);
				dto.Id = entityId;
				var result = await repository.Put(dto.ToEntity());
				if (result) return dto;
				else throw new Exception(ErrorCodes.RepoProblem);
			}
			throw new Exception(ErrorCodes.EntityNotFound);
		}
		throw new Exception(ErrorCodes.UnknownError);
	}

	public virtual async Task<bool> Delete(long entityId) {
		if (RequestIsValid(RequestType.Delete, entityId: entityId)) {
			var entity = await repository.GetById(entityId);
		if (entity != null) {
			if (!EntityBelongsToUser(entity)) throw new Exception(ErrorCodes.EntityDoesNotBelongToUser);
			var result = await repository.DeleteById(entityId);
			return result;
		}
		throw new Exception(ErrorCodes.EntityNotFound);
	}
		throw new Exception(ErrorCodes.UnknownError);
	}

	public abstract bool RequestIsValid(RequestType requestType, long? entityId = null, TDto dto = null);
	public abstract bool EntityBelongsToUser(TEntity entity);
	public abstract TDto ToDTO(TEntity entity);
	public abstract IList<TDto> ToDTOs(IList<TEntity> entities);
}
