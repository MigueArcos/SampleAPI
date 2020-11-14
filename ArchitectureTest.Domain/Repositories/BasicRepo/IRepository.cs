﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Repositories.BasicRepo {
	public interface IRepository<TEntity> where TEntity : class {
		Task<TEntity> Post(TEntity entity);
		Task<bool> Put(TEntity entity);
		Task<List<TEntity>> Get(Expression<Func<TEntity, bool>> whereFilters = null);
		Task<TEntity> GetById(long id);
		Task<bool> DeleteById(long id);
	}
}
