using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;

public interface ICrudService<TEntity> where TEntity : BaseEntity<string> 
{
    bool EntityBelongsToUser(TEntity entity);

    AppError? RequestIsValid(CrudOperation crudOperation, string? entityId = null, TEntity? entity = null);

    EntityCrudSettings CrudSettings { get; set; }

    Task<Result<TEntity, AppError>> GetById(string entityId);

    Task<Result<TEntity, AppError>> Create(TEntity entity);

    Task<Result<TEntity, AppError>> Update(string entityId, TEntity entity);

    Task<AppError?> DeleteById(string entityId);
}
