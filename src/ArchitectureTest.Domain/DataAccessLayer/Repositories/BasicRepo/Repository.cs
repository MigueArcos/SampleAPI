using ArchitectureTest.Data.Database.SQLServer;
using ArchitectureTest.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;

public class Repository<TEntity> : IRepository<long, TEntity> 
	where TEntity : class
{
	protected readonly DatabaseContext _dbContext;
	protected readonly DbSet<TEntity> _dbSet;
	public Repository(DatabaseContext dbContext) {
		_dbContext = dbContext;
		_dbSet = dbContext.Set<TEntity>();
	}

	public virtual async Task<TEntity> Add(TEntity entity) {
		_dbSet.Add(entity);
		await _dbContext.SaveChangesAsync();
		return entity;
	}

	public virtual async Task<bool> Update(TEntity entity) {
		_dbSet.Update(entity);
		long saveResult = await _dbContext.SaveChangesAsync();
		return saveResult > 0;
	}

	public virtual async Task<bool> DeleteById(long id) {
		TEntity? entity = _dbSet.Find(id) ?? throw new Exception(ErrorCodes.EntityNotFound);
		
        _dbSet.Remove(entity);
		long saveResult = await _dbContext.SaveChangesAsync();
		return saveResult > 0;
	}

    public virtual Task<IList<TEntity>> Find(Expression<Func<TEntity, bool>>? whereFilters = null) {
		Task<List<TEntity>> results = (whereFilters != null ? _dbSet.Where(whereFilters) : _dbSet).ToListAsync();
        return results.ContinueWith<IList<TEntity>>(t => t.Result, TaskContinuationOptions.ExecuteSynchronously);
	}

	public virtual Task<TEntity?> FindSingle(Expression<Func<TEntity, bool>> whereFilters) {
        return _dbSet.FirstOrDefaultAsync(whereFilters);
	}

	public virtual Task<TEntity?> GetById(long id) {
		return _dbSet.FindAsync(id).AsTask();
	}
}
