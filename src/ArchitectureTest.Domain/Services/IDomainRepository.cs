using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;

namespace ArchitectureTest.Domain.Services;

public interface IDomainRepository<TEntity> 
    where TEntity : BaseEntity<long>
{
    Task<TEntity> Add(TEntity entity);
    Task<bool> Update(TEntity entity);
    Task<bool> DeleteById(long id);
    Task<IList<TEntity>> Find(Func<TEntity, bool>? whereFilters = null);
    Task<TEntity?> FindSingle(Func<TEntity, bool> whereFilters);
    Task<TEntity?> GetById(long id);
}
