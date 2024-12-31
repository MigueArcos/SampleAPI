using System.Collections.Generic;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Models;
using System;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService;

public abstract class EntityCrudService<TEntity> : ICrudService<TEntity> 
    where TEntity : BaseEntity<string>
{
    protected readonly IRepository<TEntity> _repository;
    protected readonly IUnitOfWork _unitOfWork;
    public EntityCrudSettings CrudSettings { get; set; } = new EntityCrudSettings {
        ValidateEntityBelongsToUser = false
    };

    public EntityCrudService(IUnitOfWork unitOfWork){
        _repository = unitOfWork.Repository<TEntity>();
        _unitOfWork = unitOfWork;
    }

    public virtual async Task<Result<TEntity, AppError>> Create(TEntity inputEntity)
    {
        var requestError = RequestIsValid(CrudOperation.Create, entity: inputEntity);
        if (requestError is null) {
            inputEntity.Id = Guid.CreateVersion7().ToString("N");
            inputEntity.CreationDate = DateTime.Now;
            inputEntity.ModificationDate = null;
            await _repository.Create(inputEntity).ConfigureAwait(false);
            return inputEntity;
        }
        return requestError;
    }

    public virtual async Task<Result<TEntity, AppError>> GetById(string entityId)
    {
        if (RequestIsValid(CrudOperation.ReadById, entityId: entityId) is AppError requestError && requestError is not null)
            return requestError;
        
        var entity = await _repository.GetById(entityId).ConfigureAwait(false);
        if (entity == null)
            return new AppError(ErrorCodes.EntityNotFound);

        if (!EntityBelongsToUser(entity)) 
            return new AppError(ErrorCodes.EntityDoesNotBelongToUser);

        return entity;    
    }

    public virtual async Task<Result<TEntity, AppError>> Update(string entityId, TEntity inputEntity)
    {
        if (RequestIsValid(CrudOperation.Update, entityId, inputEntity) is AppError requestError && requestError is not null)
            return requestError;
        
        var entity = await _repository.GetById(entityId).ConfigureAwait(false);

        if (entity == null)
            return new AppError(ErrorCodes.EntityNotFound);

        if (!EntityBelongsToUser(entity))
            return new AppError(ErrorCodes.EntityDoesNotBelongToUser);

        inputEntity.Id = entityId;
        inputEntity.CreationDate = entity.CreationDate;
        inputEntity.ModificationDate = DateTime.Now;
        await _repository.Update(inputEntity).ConfigureAwait(false);
        
        return inputEntity;
    }

    public virtual async Task<AppError?> DeleteById(string entityId)
    {
        if (RequestIsValid(CrudOperation.Delete, entityId: entityId) is AppError requestError && requestError is not null)
            return requestError;

        var entity = await _repository.GetById(entityId).ConfigureAwait(false);
        if (entity == null)
            return new AppError(ErrorCodes.EntityNotFound);
        
        if (!EntityBelongsToUser(entity))
            return new AppError(ErrorCodes.EntityDoesNotBelongToUser);
        
        await _repository.DeleteById(entityId).ConfigureAwait(false);

        return null;
    }

    public virtual AppError? RequestIsValid(CrudOperation crudOperation, string? entityId = null, TEntity? entity = null)
    {
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
    public abstract Dictionary<CrudOperation, List<(Func<TEntity?, string?, bool>, string)>> ValidationsByOperation { get; }
}
