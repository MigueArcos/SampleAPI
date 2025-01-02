using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;

namespace ArchitectureTest.Domain.Services;

public interface IRepository<TEntity> 
    where TEntity : BaseEntity<string>
{
    Task Create(TEntity entity, bool autoSave = true);
    Task Update(TEntity entity, bool autoSave = true);
    Task DeleteById(string id, bool autoSave = true);
    Task<IList<TEntity>> Find(Expression<Func<TEntity, bool>>? whereFilters = null);
    Task<TEntity?> FindSingle(Expression<Func<TEntity, bool>> whereFilters);
    Task<TEntity?> GetById(string id);
}
