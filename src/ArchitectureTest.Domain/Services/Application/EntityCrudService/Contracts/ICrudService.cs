using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Enums;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;

public interface ICrudService<TEntity, TDto>
    where TEntity : BaseEntity<string>
    where TDto : class
{
    bool EntityBelongsToUser(TEntity entity);

    AppError? RequestIsValid(CrudOperation crudOperation, string? entityId = null, TDto? dto = null);

    EntityCrudSettings CrudSettings { get; set; }

    Task<Result<TDto, AppError>> GetById(string entityId);

    Task<Result<(TDto Entity, string Id), AppError>> Create(TDto entity);

    Task<Result<TDto, AppError>> Update(string entityId, TDto entity);

    Task<AppError?> DeleteById(string entityId);
}
