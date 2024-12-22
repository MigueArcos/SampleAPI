using ArchitectureTest.Databases.MySql;
using ArchitectureTest.Databases.MySql.Entities;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Infrastructure.ExpressionUtils;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArchitectureTest.Infrastructure.SqlEFCore.MySql;

public class ChecklistMySqlRepository : SqlRepository<ChecklistEntity, Checklist> {
    public ChecklistMySqlRepository(DatabaseContext dbContext, IMapper mapper) : base(dbContext, mapper) {
    }
    public override Task<IList<ChecklistEntity>> Find(Expression<Func<ChecklistEntity, bool>>? whereFilters = null) {
        var queryable = _dbSet.Include("ChecklistDetails");

        Task<List<Checklist>> results = (
            whereFilters != null ? _dbSet.Where(whereFilters.ReplaceLambdaParameter<ChecklistEntity, Checklist>()) : _dbSet
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
        return _dbSet.FirstOrDefaultAsync(whereForDatabase)
            .AvoidTracking(_dbSet)
            .ContinueWith(t => _mapper.Map<ChecklistEntity?>(t.Result), TaskContinuationOptions.ExecuteSynchronously);
    }
}
