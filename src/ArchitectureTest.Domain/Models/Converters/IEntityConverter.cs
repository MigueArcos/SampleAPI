using System.Collections.Generic;

namespace ArchitectureTest.Domain.Models.Converters;

public interface IEntityConverter<TEntity> where TEntity : class {
    TEntity ToEntity();
}

public interface IChildEntityConverter<TEntity> where TEntity : class {
    IList<TEntity>? GetChildEntities();
}
