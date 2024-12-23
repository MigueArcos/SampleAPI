using System.Collections.Generic;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Models;
using System;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.NewImpl.Contracts;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.NewImpl;

public abstract class EntityCrudService<TEntity> : ICrudService<TEntity> 
    where TEntity : BaseEntity<long>
{
    protected readonly IDomainRepository<TEntity> _repository;
    protected readonly IDomainUnitOfWork _unitOfWork;
    public EntityCrudSettings CrudSettings { get; set; } = new EntityCrudSettings {
        ValidateEntityBelongsToUser = false
    };

    public EntityCrudService(IDomainUnitOfWork unitOfWork) {
        _repository = unitOfWork.Repository<TEntity>();
        _unitOfWork = unitOfWork;
    }
    public virtual async Task<Result<TEntity, AppError>> Add(TEntity inputEntity) {
        var requestError = RequestIsValid(CrudOperation.Create, entity: inputEntity);
        if (requestError is null) {
            var entity = await _repository.Add(inputEntity).ConfigureAwait(false);
            return entity;
        }
        return requestError;
    }

    public virtual async Task<Result<TEntity, AppError>> GetById(long entityId) {
        if (RequestIsValid(CrudOperation.ReadById, entityId: entityId) is AppError requestError && requestError is not null)
            return requestError;
        
        var entity = await _repository.GetById(entityId).ConfigureAwait(false);
        if (entity == null)
            return new AppError(ErrorCodes.EntityNotFound);

        if (!EntityBelongsToUser(entity)) 
            return new AppError(ErrorCodes.EntityDoesNotBelongToUser);

        return entity;    
    }

    public virtual async Task<Result<TEntity, AppError>> Update(long entityId, TEntity inputEntity) {
        if (RequestIsValid(CrudOperation.Update, entityId, inputEntity) is AppError requestError && requestError is not null)
            return requestError;
        
        var entity = await _repository.GetById(entityId).ConfigureAwait(false);

        if (entity == null)
            return new AppError(ErrorCodes.EntityNotFound);
            
        if (!EntityBelongsToUser(entity))
            return new AppError(ErrorCodes.EntityDoesNotBelongToUser);

        inputEntity.Id = entityId;
        var result = await _repository.Update(inputEntity).ConfigureAwait(false);

        if (result)
            return inputEntity;
        else
            return new AppError(ErrorCodes.RepoProblem);
    }

    public virtual async Task<AppError?> Delete(long entityId) {
        if (RequestIsValid(CrudOperation.Delete, entityId: entityId) is AppError requestError && requestError is not null)
            return requestError;

        var entity = await _repository.GetById(entityId).ConfigureAwait(false);
        if (entity == null)
            return new AppError(ErrorCodes.EntityNotFound);
        
        if (!EntityBelongsToUser(entity))
            return new AppError(ErrorCodes.EntityDoesNotBelongToUser);
        
        var result = await _repository.DeleteById(entityId).ConfigureAwait(false);

        if (result)
            return null;
        else
            return new AppError(ErrorCodes.RepoProblem);
    }

    public virtual AppError? RequestIsValid(CrudOperation crudOperation, long? entityId = null, TEntity? entity = null) {
        bool found = ValidationsByOperation.TryGetValue(crudOperation, out var validations);
        if (!found)
            return new AppError(ErrorCodes.IncorrectInputData);
        
        foreach (var validation in validations!){
            var (validationFailedFunc, errorCode) = validation;
            if (validationFailedFunc(entity, entityId))
                return new AppError(errorCode);
        }

        return null;
    }

    public abstract bool EntityBelongsToUser(TEntity entity);
    public abstract Dictionary<CrudOperation, List<(Func<TEntity?, long?, bool>, string)>> ValidationsByOperation { get; }
}
