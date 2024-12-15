using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using ArchitectureTest.Domain.Models.Converters;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService.Contracts;
using ArchitectureTest.Domain.Models.Enums;
using System;

namespace ArchitectureTest.Domain.ServiceLayer.EntityCrudService;

public abstract class EntityCrudService<TEntity, TDto> : ICrudService<TEntity, TDto> 
    where TEntity : class
    where TDto : BasicDTO<long>, IEntityConverter<TEntity> 
{
    protected readonly IRepository<long, TEntity> _repository;
    protected readonly IUnitOfWork _unitOfWork;
    public EntityCrudSettings CrudSettings { get; set; } = new EntityCrudSettings {
        ValidateEntityBelongsToUser = false
    };

    public EntityCrudService(IUnitOfWork unitOfWork) {
        _repository = unitOfWork.Repository<TEntity>();
        _unitOfWork = unitOfWork;
    }
    public virtual async Task<Result<TDto, AppError>> Add(TDto dto) {
        var requestError = RequestIsValid(CrudOperation.Create, dto: dto);
        if (requestError is null) {
            var entity = await _repository.Add(dto.ToEntity()).ConfigureAwait(false);
            return ToDTO(entity);
        }
        return requestError;
    }

    public virtual async Task<Result<TDto, AppError>> GetById(long entityId) {
        if (RequestIsValid(CrudOperation.ReadById, entityId: entityId) is AppError requestError && requestError is not null)
            return requestError;
        
        var entity = await _repository.GetById(entityId).ConfigureAwait(false);
        if (entity == null)
            return new AppError(ErrorCodes.EntityNotFound);

        if (!EntityBelongsToUser(entity)) 
            return new AppError(ErrorCodes.EntityDoesNotBelongToUser);

        return ToDTO(entity);    
    }

    public virtual async Task<Result<TDto, AppError>> Update(long entityId, TDto dto) {
        if (RequestIsValid(CrudOperation.Update, entityId, dto) is AppError requestError && requestError is not null)
            return requestError;
        
        var entity = await _repository.GetById(entityId).ConfigureAwait(false);

        if (entity == null)
            return new AppError(ErrorCodes.EntityNotFound);
            
        if (!EntityBelongsToUser(entity))
            return new AppError(ErrorCodes.EntityDoesNotBelongToUser);

        dto.Id = entityId;
        var result = await _repository.Update(dto.ToEntity()).ConfigureAwait(false);

        if (result)
            return dto;
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

    public virtual AppError? RequestIsValid(CrudOperation crudOperation, long? entityId = null, TDto? dto = null) {
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

    public abstract bool EntityBelongsToUser(TEntity entity);
    public abstract TDto ToDTO(TEntity entity);
    public abstract IList<TDto> ToDTOs(IList<TEntity> entities);
    public abstract Dictionary<CrudOperation, List<(Func<TDto?, long?, bool>, string)>> ValidationsByOperation { get; }
}
