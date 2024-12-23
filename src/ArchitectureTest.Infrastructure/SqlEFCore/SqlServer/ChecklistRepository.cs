using ArchitectureTest.Databases.SqlServer;
using ArchitectureTest.Infrastructure.ExpressionUtils;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using DomainEntities = ArchitectureTest.Domain.Entities;
using Database = ArchitectureTest.Databases.SqlServer.Entities;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;

public class ChecklistSqlServerRepository : SqlRepository<DomainEntities.Checklist, Database.Checklist> {
    public ChecklistSqlServerRepository(DatabaseContext dbContext, IMapper mapper) : base(dbContext, mapper) {
    }

    public override Task<IList<DomainEntities.Checklist>> Find(
        Expression<Func<DomainEntities.Checklist, bool>>? whereFilters = null
    ){
        var queryable = _dbSet.Include("ChecklistDetails");

        Task<List<Database.Checklist>> results = (
            whereFilters != null ? 
                queryable.Where(whereFilters.ReplaceLambdaParameter<DomainEntities.Checklist, Database.Checklist>()) :
                queryable
        ).ToListAsync();

        return results.ContinueWith<IList<DomainEntities.Checklist>>(
            t => t.Result?.Select(e => _mapper.Map<DomainEntities.Checklist>(e)).ToList() ?? [],
            TaskContinuationOptions.ExecuteSynchronously
        );
    }

    public override Task<DomainEntities.Checklist?> GetById(long id)
    {
        return _dbSet.Include("ChecklistDetails").FirstOrDefaultAsync(e => e.Id == id).AvoidTracking(_dbSet)
            .ContinueWith(t => _mapper.Map<DomainEntities.Checklist?>(t.Result), TaskContinuationOptions.ExecuteSynchronously);
    }

    public override Task<DomainEntities.Checklist?> FindSingle(Expression<Func<DomainEntities.Checklist, bool>> whereFilters)
    {
        var whereForDatabase = whereFilters.ReplaceLambdaParameter<DomainEntities.Checklist, Database.Checklist>();

        return _dbSet.Include("ChecklistDetails").FirstOrDefaultAsync(whereForDatabase)
            .AvoidTracking(_dbSet)
            .ContinueWith(t => _mapper.Map<DomainEntities.Checklist?>(t.Result), TaskContinuationOptions.ExecuteSynchronously);
    }
}
