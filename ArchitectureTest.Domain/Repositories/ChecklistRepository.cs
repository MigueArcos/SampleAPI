using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Domain.Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Repositories {
	public class ChecklistRepository : Repository<Checklist>{
		public ChecklistRepository(DatabaseContext dbContext) : base(dbContext) {
		}
		public override Task<List<Checklist>> Get(Expression<Func<Checklist, bool>> whereFilters = null) {
			return whereFilters != null ? dbSet.Include("ChecklistDetail").Where(whereFilters).ToListAsync() : dbSet.Include("ChecklistDetail").ToListAsync();
		}

		public override Task<Checklist> GetById(long id) {
			return dbSet.Include("ChecklistDetail").FirstOrDefaultAsync(e => e.Id == id);
		}
	}
}
