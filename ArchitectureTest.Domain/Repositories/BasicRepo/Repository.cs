using ArchitectureTest.Data.Database.SQLServer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Repositories.BasicRepo {
	public class Repository<TEntity> : IRepository<TEntity> where TEntity : class {
		protected readonly DatabaseContext dbContext;
		protected readonly DbSet<TEntity> dbSet;
		public Repository(DatabaseContext dbContext) {
			this.dbContext = dbContext;
			dbSet = dbContext.Set<TEntity>();
		}
		//We should never expose real databaseErrors, so we will catch those exception and rethrow an unknown exception
		public virtual async Task<bool> DeleteById(long id) {
			TEntity entity = dbSet.Find(id);
			var deletedEntity = dbSet.Remove(entity);
			await dbContext.SaveChangesAsync();
			return deletedEntity.State == EntityState.Deleted;
		}

		public virtual Task<List<TEntity>> Get(Expression<Func<TEntity, bool>> whereFilters = null) {
			return whereFilters != null ? dbSet.Where(whereFilters).ToListAsync() : dbSet.ToListAsync();
		}

		public virtual Task<TEntity> GetById(long id) {
			return dbSet.FindAsync(id);
		}

		public virtual async Task<TEntity> Post(TEntity entity) {
			dbSet.Add(entity);
			await dbContext.SaveChangesAsync();
			return entity;
		}

		public virtual async Task<bool> Put(TEntity entity) {
			dbSet.Update(entity);
			long saveResult = await dbContext.SaveChangesAsync();
			return saveResult > 0;
		}
	}
}
