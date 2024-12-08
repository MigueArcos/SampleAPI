using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.DataAccessLayer.Repositories; 
public class ChecklistRepository : Repository<Checklist> {
	public ChecklistRepository(DatabaseContext dbContext) : base(dbContext) {
	}
	public override Task<IList<Checklist>> Get(Expression<Func<Checklist, bool>> whereFilters = null) {
		return Task.FromResult<IList<Checklist>>(
			whereFilters != null ? 
				dbSet.Include("ChecklistDetail").Where(whereFilters).ToList() : 
				dbSet.Include("ChecklistDetail").ToList()
		);
	}

	public override Task<Checklist> GetById(long id) {
		return dbSet.Include("ChecklistDetail").FirstOrDefaultAsync(e => e.Id == id);
	}
}
