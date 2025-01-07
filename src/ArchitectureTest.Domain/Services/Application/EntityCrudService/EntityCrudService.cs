using System.Collections.Generic;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Models;
using System;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using AutoMapper;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService;

public abstract class EntityCrudService<TEntity, TDto> : ICrudService<TEntity, TDto> 
    where TEntity : BaseEntity<string>
    where TDto : class
{
    protected readonly IRepository<TEntity> _repository;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;
    protected bool _autoSave = true;

    public EntityCrudSettings CrudSettings { get; set; } = new EntityCrudSettings {
        ValidateEntityBelongsToUser = false
    };

    public EntityCrudService(IUnitOfWork unitOfWork, IMapper mapper){
        _repository = unitOfWork.Repository<TEntity>();
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public virtual async Task<Result<(TDto Entity, string Id), AppError>> Create(TDto input)
    {
        var requestError = RequestIsValid(CrudOperation.Create, dto: input);
        if (requestError is null) {
            var inputEntity = _mapper.Map<TEntity>(input);
            inputEntity.Id = Guid.CreateVersion7().ToString("N");
            inputEntity.CreationDate = DateTime.Now;
            inputEntity.ModificationDate = null;
            await _repository.Create(inputEntity, _autoSave).ConfigureAwait(false);
            return (_mapper.Map<TDto>(inputEntity), inputEntity.Id);
        }
        return requestError;
    }

    public virtual async Task<Result<TDto, AppError>> GetById(string entityId)
    {
        var entityResult = await ValidateAndGetEntity(CrudOperation.ReadById, entityId)
            .ConfigureAwait(false);

        if (entityResult.Error != null)
            return entityResult.Error;

        var entity = entityResult.Value;

        return _mapper.Map<TDto>(entity);    
    }

    public virtual async Task<Result<TDto, AppError>> Update(string entityId, TDto input)
    {
        var entityResult = await ValidateAndGetEntity(CrudOperation.Update, entityId, dto: input)
            .ConfigureAwait(false);

        if (entityResult.Error != null)
            return entityResult.Error;

        var entity = entityResult.Value!;

        var inputAsEntity = _mapper.Map<TEntity>(input);
        inputAsEntity.Id = entityId;
        inputAsEntity.CreationDate = entity.CreationDate;
        inputAsEntity.ModificationDate = DateTime.Now;
        await _repository.Update(inputAsEntity, _autoSave).ConfigureAwait(false);
        
        return _mapper.Map<TDto>(inputAsEntity);
    }

    public virtual async Task<AppError?> DeleteById(string entityId)
    {
        var entityResult = await ValidateAndGetEntity(CrudOperation.Delete, entityId)
            .ConfigureAwait(false);

        if (entityResult.Error != null)
            return entityResult.Error;
        
        await _repository.DeleteById(entityId, _autoSave).ConfigureAwait(false);

        return null;
    }
    
    protected async Task<Result<TEntity, AppError>> ValidateAndGetEntity(CrudOperation crudOperation, string entityId, TDto? dto = null)
    {
        if (RequestIsValid(crudOperation, entityId, dto) is AppError requestError && requestError is not null)
            return requestError;
        
        var entity = await _repository.GetById(entityId).ConfigureAwait(false);
        if (entity == null)
            return new AppError(ErrorCodes.EntityNotFound);

        if (!EntityBelongsToUser(entity)) 
            return new AppError(ErrorCodes.EntityDoesNotBelongToUser);
        
        return entity;
    }

    public virtual AppError? RequestIsValid(CrudOperation crudOperation, string? entityId = null, TDto? dto = null)
    {
        bool found = ValidationsByOperation.TryGetValue(crudOperation, out var validations);
        if (!found)
            return new AppError(ErrorCodes.IncorrectInputData);
        
        foreach (var validation in validations!){
            var (validationFailedFunc, errorCode) = validation;
            if (validationFailedFunc(dto, entityId))
                return new AppError(errorCode);
        }

        return null;
    }

    protected void ResetAutoSaveOption() => _autoSave = true;

    public abstract bool EntityBelongsToUser(TEntity entity);
    public abstract Dictionary<CrudOperation, List<(Func<TDto?, string?, bool>, string)>> ValidationsByOperation { get; }
}
