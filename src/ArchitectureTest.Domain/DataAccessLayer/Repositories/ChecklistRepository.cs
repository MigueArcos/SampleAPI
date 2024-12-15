using ArchitectureTest.Data.Database.SQLServer;
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
    public override Task<IList<Checklist>> Find(Expression<Func<Checklist, bool>>? whereFilters = null) {
        var queryable = _dbSet.Include("ChecklistDetails");
        Task<List<Checklist>> results = (whereFilters != null ? queryable.Where(whereFilters) : queryable).ToListAsync();
        return results.ContinueWith<IList<Checklist>>(t => t.Result, TaskContinuationOptions.ExecuteSynchronously);
    }

    public override Task<Checklist?> GetById(long id) {
        return _dbSet.Include("ChecklistDetails").FirstOrDefaultAsync(e => e.Id == id).AvoidTracking(_dbSet);
    }
}
