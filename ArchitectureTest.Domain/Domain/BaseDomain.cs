using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.Repositories.BasicRepo;
using ArchitectureTest.Domain.UnitOfWork;
using ArchitectureTest.Domain.Contracts;
using ArchitectureTest.Domain.StatusCodes;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ArchitectureTest.Domain.Models;

namespace ArchitectureTest.Domain.Domain {
	public abstract class BaseDomain<TEntity, TDto> : IDtoConverter<TEntity, TDto> where TEntity : class where TDto : BasicDTO, IEntityConverter<TEntity> {
		protected readonly IRepository<TEntity> repository;
		protected readonly IUnitOfWork unitOfWork;
		public BaseDomain(IUnitOfWork unitOfWork) {
			this.repository = unitOfWork.Repository<TEntity>();
			this.unitOfWork = unitOfWork;
		}
		public virtual async Task<TDto> Post(TDto dto) {
			try{
				if (RequestIsValid(RequestType.Post, dto: dto)) {
					var entity = await repository.Post(dto.ToEntity());
					return ToDTO(entity);
				}
				throw ErrorStatusCode.UnknownError;
			}
			catch (Exception exception){
				throw Utils.HandleException(exception);
			}
		}

		public virtual async Task<TDto> GetById(long entityId, long? userId = null) {
			try {
				if (RequestIsValid(RequestType.Get, entityId: entityId)) {
					var entity = await repository.GetById(entityId);
					if (entity != null){
						if (userId != null && !EntityBelongsToUser(entity, userId.Value)) throw ErrorStatusCode.EntityDoesNotBelongToUser;
						return ToDTO(entity);
					}
					throw ErrorStatusCode.EntityNotFound;
				}
				throw ErrorStatusCode.UnknownError;
			}
			catch (Exception exception) {
				throw Utils.HandleException(exception);
			}
		}

		public virtual async Task<TDto> Put(long entityId, TDto dto, long? userId = null) {
			try {
				if (RequestIsValid(RequestType.Put, entityId: entityId, dto: dto)) {
					var entity = await repository.GetById(entityId);
					if (entity != null && userId != null && !EntityBelongsToUser(entity, userId.Value)) throw ErrorStatusCode.EntityDoesNotBelongToUser;
					dto.Id = entityId;
					var result = await repository.Put(dto.ToEntity());
					if (result) return dto;
					else throw ErrorStatusCode.RepoProblem;
				}
				throw ErrorStatusCode.UnknownError;
			}
			catch (Exception exception) {
				throw Utils.HandleException(exception);
			}
		}

		public virtual async Task<bool> Delete(long entityId, long? userId = null) {
			try {
				if (RequestIsValid(RequestType.Delete, entityId: entityId)) {
					var entity = await repository.GetById(entityId);
					if (entity != null && userId != null && !EntityBelongsToUser(entity, userId.Value)) throw ErrorStatusCode.EntityDoesNotBelongToUser;
					var result = await repository.DeleteById(entityId);
					return result;
				}
				throw ErrorStatusCode.UnknownError;
			}
			catch (Exception exception) {
				throw Utils.HandleException(exception);
			}
		}

		public abstract bool RequestIsValid(RequestType requestType, long? entityId = null, TDto dto = null);
		public abstract bool EntityBelongsToUser(TEntity entity, long userId);
		public abstract TDto ToDTO(TEntity entity);
		public abstract IList<TDto> ToDTOs(IList<TEntity> entities);
	}
}
