using System.Collections.Generic;
using System.Threading.Tasks;
using ArchitectureTest.Data.Enums;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Converters;

namespace ArchitectureTest.Domain.ServiceLayer.EntityCrudService.Contracts;

public interface ICrudService<TEntity, TDto> where TEntity : class where TDto : BasicDTO, IEntityConverter<TEntity> {
    EntityCrudSettings CrudSettings { get; set; }
    Task<bool> Delete(long entityId);
    bool EntityBelongsToUser(TEntity entity);
    Task<TDto> GetById(long entityId);
    Task<TDto> Add(TDto dto);
    Task<TDto> Update(long entityId, TDto dto);
    bool RequestIsValid(RequestType requestType, long? entityId = null, TDto? dto = null);
    TDto ToDTO(TEntity entity);
    IList<TDto> ToDTOs(IList<TEntity> entities);
}
