using ArchitectureTest.Infrastructure.ExpressionUtils;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using DomainEntities = ArchitectureTest.Domain.Entities;

namespace ArchitectureTest.Infrastructure.SqlEFCore.Common;

public class BaseChecklistRepository<DbType> : SqlRepository<DomainEntities.Checklist, DbType> where DbType : class {
    private readonly Func<long, Expression<Func<DbType, bool>>> _findByIdPredicate;
    public BaseChecklistRepository(
        DbContext dbContext, IMapper mapper, Func<long, Expression<Func<DbType, bool>>> findByIdPredicate
    ) : base(dbContext, mapper) {
        _findByIdPredicate = findByIdPredicate;
    }

    public override Task<IList<DomainEntities.Checklist>> Find(
        Expression<Func<DomainEntities.Checklist, bool>>? whereFilters = null
    ){
        var queryable = _dbSet.Include("ChecklistDetails");

        Task<List<DbType>> results = (
            whereFilters != null ? 
                queryable.Where(whereFilters.ReplaceLambdaParameter<DomainEntities.Checklist, DbType>()) :
                queryable
        ).ToListAsync();

        return results
            .ContinueWith<IList<DomainEntities.Checklist>>(
                t => t.Result?.Select(e => _mapper.Map<DomainEntities.Checklist>(e)).ToList() ?? [],
                TaskContinuationOptions.ExecuteSynchronously
            );
    }

    public override Task<DomainEntities.Checklist?> GetById(long id)
    {
        return _dbSet
            .Include("ChecklistDetails")
            .FirstOrDefaultAsync(_findByIdPredicate(id))
            .AvoidTracking(_dbSet)
            .ContinueWith(t => _mapper.Map<DomainEntities.Checklist?>(t.Result), TaskContinuationOptions.ExecuteSynchronously);
    }

    public override Task<DomainEntities.Checklist?> FindSingle(Expression<Func<DomainEntities.Checklist, bool>> whereFilters)
    {
        var whereForDatabase = whereFilters.ReplaceLambdaParameter<DomainEntities.Checklist, DbType>();

        return _dbSet
            .Include("ChecklistDetails")
            .FirstOrDefaultAsync(whereForDatabase)
            .AvoidTracking(_dbSet)
            .ContinueWith(t => _mapper.Map<DomainEntities.Checklist?>(t.Result), TaskContinuationOptions.ExecuteSynchronously);
    }
}
