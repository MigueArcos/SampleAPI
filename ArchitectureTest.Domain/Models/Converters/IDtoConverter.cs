using System.Collections.Generic;

namespace ArchitectureTest.Domain.Models.Converters {
	public interface IDtoConverter<TEntity, TDto> {
		TDto ToDTO(TEntity entity);
		IList<TDto> ToDTOs(IList<TEntity> entities);
	}
}
