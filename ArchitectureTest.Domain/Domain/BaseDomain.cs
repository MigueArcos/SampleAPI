using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Data.Enums;
using ArchitectureTest.Data.Repositories.BasicRepo;
using ArchitectureTest.Data.UnitOfWork;
using ArchitectureTest.Domain.Contracts;
using ArchitectureTest.Data.StatusCodes;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ArchitectureTest.Domain.Models;

namespace ArchitectureTest.Domain.Domain {
	public abstract class BaseDomain<TEntity, TDto> : IDtoConverter<TEntity, TDto> where TEntity : Entity where TDto : BasicDTO, IEntityConverter<TEntity> {
		protected readonly IRepository<TEntity> repository;
		protected readonly IUnitOfWork unitOfWork;
		public BaseDomain(IRepository<TEntity> repository, IUnitOfWork unitOfWork) {
			this.repository = repository;
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
				throw DefaultCatchHandler(exception);
			}
		}

		public virtual async Task<TDto> GetById(long entityId) {
			try {
				if (RequestIsValid(RequestType.Get, entityId: entityId)) {
					var entity = await repository.GetById(entityId);
					return ToDTO(entity);
				}
				throw ErrorStatusCode.UnknownError;
			}
			catch (Exception exception) {
				throw DefaultCatchHandler(exception);
			}
		}

		public virtual async Task<TDto> Put(long entityId, TDto dto) {
			try {
				if (RequestIsValid(RequestType.Put, entityId: entityId, dto: dto)) {
					dto.Id = entityId;
					var result = await repository.Put(dto.ToEntity());
					if (result) return dto;
					else throw ErrorStatusCode.RepoProblem;
				}
				throw ErrorStatusCode.UnknownError;
			}
			catch (Exception exception) {
				throw DefaultCatchHandler(exception);
			}
		}

		public virtual async Task<bool> Delete(long entityId) {
			try {
				if (RequestIsValid(RequestType.Delete, entityId: entityId)) {
					var result = await repository.DeleteById(entityId);
					return result;
				}
				throw ErrorStatusCode.UnknownError;
			}
			catch (Exception exception) {
				throw DefaultCatchHandler(exception);
			}
		}

		protected ErrorStatusCode DefaultCatchHandler(Exception exception){
			if (exception is ErrorStatusCode) {
				return exception as ErrorStatusCode;
			}
			else {
				//We should never expose real exceptions, so we will catch all unknown exceptions (DatabaseErrors, Null Errors, Index errors, etc...) and rethrow an unknown exception after log
				Console.WriteLine(exception);
				return ErrorStatusCode.UnknownError;
			}
		}
		public abstract bool RequestIsValid(RequestType requestType, long? entityId = null, TDto dto = null);
		public abstract TDto ToDTO(TEntity entity);
		public abstract IList<TDto> ToDTOs(IList<TEntity> entities);
	}
}
