using System.Collections.Generic;
using System.Threading.Tasks;
using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Converters;

namespace ArchitectureTest.Domain.ServiceLayer.EntityCrudService.Contracts;

public interface ICrudService<TEntity, TDto> where TEntity : class where TDto : BasicDTO<long>, IEntityConverter<TEntity> {
    bool EntityBelongsToUser(TEntity entity);

    AppError? RequestIsValid(RequestType requestType, long? entityId = null, TDto? dto = null);

    EntityCrudSettings CrudSettings { get; set; }

    Task<Result<TDto, AppError>> GetById(long entityId);

    Task<Result<TDto, AppError>> Add(TDto dto);

    Task<Result<TDto, AppError>> Update(long entityId, TDto dto);

    Task<AppError?> Delete(long entityId);

    TDto ToDTO(TEntity entity);

    IList<TDto> ToDTOs(IList<TEntity> entities);
}
