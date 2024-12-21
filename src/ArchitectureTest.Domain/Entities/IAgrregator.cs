using System.Collections.Generic;

namespace ArchitectureTest.Domain.Entities;

public interface IAggregator<TEntity> where TEntity : BaseEntity<long> {
    IList<TEntity>? GetChildEntities();
}
