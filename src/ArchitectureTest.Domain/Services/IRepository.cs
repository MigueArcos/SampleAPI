using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;

namespace ArchitectureTest.Domain.Services;

public interface IRepository<TEntity> 
    where TEntity : BaseEntity<long>
{
    Task<TEntity> Create(TEntity entity);
    Task<bool> Update(TEntity entity);
    Task<bool> DeleteById(long id);
    Task<IList<TEntity>> Find(Expression<Func<TEntity, bool>>? whereFilters = null);
    Task<TEntity?> FindSingle(Expression<Func<TEntity, bool>> whereFilters);
    Task<TEntity?> GetById(long id);
}
