using System;
using System.Collections.Generic;
using System.Text;

namespace ArchitectureTest.Domain.Contracts {
	public interface IDtoConverter<TEntity, TDto> {
		TDto ToDTO(TEntity entity);
		IList<TDto> ToDTOs(IList<TEntity> entities);
	}
}
