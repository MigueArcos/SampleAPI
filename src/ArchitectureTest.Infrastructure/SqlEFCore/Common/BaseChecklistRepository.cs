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

public abstract class BaseChecklistRepository<DbType, DetailType> : SqlRepository<DomainEntities.Checklist, DbType> 
    where DetailType : class
    where DbType : class 
{
    private readonly DbSet<DetailType> _checklistDetailsDbSet;
    public BaseChecklistRepository(DbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
        _checklistDetailsDbSet = dbContext.Set<DetailType>();
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

    public override Task<DomainEntities.Checklist?> GetById(string id)
    {
        return _dbSet
            .Include("ChecklistDetails")
            .FirstOrDefaultAsync(BuildFindByIdPredicate(id))
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

    public async Task<int> DeleteDetails(string checklistId, bool autoSave = true)
    {
        int deleteCount = await _checklistDetailsDbSet.Where(BuildFindDetailByChecklistIdPredicate(checklistId))
            .ExecuteDeleteAsync().ConfigureAwait(false);
        
        if (autoSave)
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return deleteCount;
    }

    public abstract Expression<Func<DbType, bool>> BuildFindByIdPredicate(string id);

    public abstract Expression<Func<DetailType, bool>> BuildFindDetailByChecklistIdPredicate(string checklistId);
}
