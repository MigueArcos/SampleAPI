using System.Collections.Generic;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;

public interface ICrudService<TEntity> where TEntity : BaseEntity<long> 
{
    bool EntityBelongsToUser(TEntity entity);

    AppError? RequestIsValid(CrudOperation crudOperation, long? entityId = null, TEntity? entity = null);

    EntityCrudSettings CrudSettings { get; set; }

    Task<Result<TEntity, AppError>> GetById(long entityId);

    Task<Result<TEntity, AppError>> Add(TEntity entity);

    Task<Result<TEntity, AppError>> Update(long entityId, TEntity entity);

    Task<AppError?> Delete(long entityId);
}
