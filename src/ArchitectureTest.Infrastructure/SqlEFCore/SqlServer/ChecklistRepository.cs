using ArchitectureTest.Databases.SqlServer;
using ArchitectureTest.Databases.SqlServer.Entities;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Infrastructure.ExpressionUtils;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;

public class ChecklistSqlServerRepository : SqlRepository<ChecklistEntity, Checklist> {
    public ChecklistSqlServerRepository(DatabaseContext dbContext, IMapper mapper) : base(dbContext, mapper) {
    }
    public override Task<IList<ChecklistEntity>> Find(Expression<Func<ChecklistEntity, bool>>? whereFilters = null) {
        var queryable = _dbSet.Include("ChecklistDetails");

        Task<List<Checklist>> results = (
            whereFilters != null ? queryable.Where(whereFilters.ReplaceLambdaParameter<ChecklistEntity, Checklist>()) : queryable
        ).ToListAsync();

        return results.ContinueWith<IList<ChecklistEntity>>(
            t => t.Result?.Select(e => _mapper.Map<ChecklistEntity>(e)).ToList() ?? [],
            TaskContinuationOptions.ExecuteSynchronously
        );
    }

    public override Task<ChecklistEntity?> GetById(long id) {
        return _dbSet.Include("ChecklistDetails").FirstOrDefaultAsync(e => e.Id == id).AvoidTracking(_dbSet)
            .ContinueWith(t => _mapper.Map<ChecklistEntity?>(t.Result), TaskContinuationOptions.ExecuteSynchronously);
    }

    public override Task<ChecklistEntity?> FindSingle(Expression<Func<ChecklistEntity, bool>> whereFilters)
    {
        var whereForDatabase = whereFilters.ReplaceLambdaParameter<ChecklistEntity, Checklist>();

        return _dbSet.Include("ChecklistDetails").FirstOrDefaultAsync(whereForDatabase)
            .AvoidTracking(_dbSet)
            .ContinueWith(t => _mapper.Map<ChecklistEntity?>(t.Result), TaskContinuationOptions.ExecuteSynchronously);
    }
}
