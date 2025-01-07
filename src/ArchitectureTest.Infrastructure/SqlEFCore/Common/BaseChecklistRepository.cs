using ArchitectureTest.Domain.Services;
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

public abstract class BaseChecklistRepository<DbType, DetailType> : 
    SqlRepository<DomainEntities.Checklist, DbType>, IChecklistRepository
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

    // TODO: The delete operations have a weird behavior, if we delete the details using ExecuteDeleteAsync and Delete the
    // parent checklist using the Remove method (the one in the parent) it throws exception (seems related to the transaction).
    // This seems to only work if we use the same method for both remove operations (foreach => Remove or ExecuteDeleteAsync)
    // Check this question on why this method of Delete + DeleteDetails is split
    // https://chatgpt.com/share/677b2e35-f710-8000-80a4-17e992841d10
    public async Task<int> DeleteDetails(string checklistId, bool autoSave = true)
    {
        int deleteCount = await _checklistDetailsDbSet.Where(BuildFindDetailByChecklistIdPredicate(checklistId))
            .ExecuteDeleteAsync().ConfigureAwait(false);
        
        if (autoSave)
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return deleteCount;
    }

    public override async Task DeleteById(string id, bool autoSave = true)
    {
        await _dbSet.Where(BuildFindByIdPredicate(id)).ExecuteDeleteAsync().ConfigureAwait(false);
        
        if (autoSave)
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public abstract Expression<Func<DbType, bool>> BuildFindByIdPredicate(string id);

    public abstract Expression<Func<DetailType, bool>> BuildFindDetailByChecklistIdPredicate(string checklistId);
}
