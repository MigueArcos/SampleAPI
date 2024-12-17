using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Services;

public interface IRepository<TId, TEntity> 
    where TEntity : class
{
    Task<TEntity> Add(TEntity entity);
    Task<bool> Update(TEntity entity);
    Task<bool> DeleteById(TId id);
    Task<IList<TEntity>> Find(Expression<Func<TEntity, bool>>? whereFilters = null);
    Task<TEntity?> FindSingle(Expression<Func<TEntity, bool>> whereFilters);
    Task<TEntity?> GetById(TId id);
}
