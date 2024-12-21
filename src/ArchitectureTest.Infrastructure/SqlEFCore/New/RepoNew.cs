using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureTest.Databases.SqlServer;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArchitectureTest.Infrastructure.SqlEFCore;

public static class EFRepositoryExtensions {
    public static Task<T?> AvoidTracking<T>(this Task<T?> task, DbSet<T> dbSet) where T : class {
        return task.ContinueWith(t => {
            if (t.Result is not null)
                dbSet.Entry(t.Result).State = EntityState.Detached;

            return t.Result;
        }, TaskContinuationOptions.ExecuteSynchronously);
    }
}

public class SqlServerRepository<D, T> : IDomainRepository<D>
    where D : BaseEntity<long>
    where T : class
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;
    protected readonly DbSet<T> _dbSet;

    public SqlServerRepository(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _dbSet = dbContext.Set<T>();
    }

    public async Task<D> Add(D domainEntity)
    {
        var dbEntity = _mapper.Map<T>(domainEntity);
        _dbSet.Add(dbEntity);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return domainEntity;
    }

    public async Task<bool> DeleteById(long id)
    {
        T? dbEntity = await _dbSet.FindAsync(id).ConfigureAwait(false)
            ?? throw new Exception(ErrorCodes.EntityNotFound);
        
        _dbSet.Remove(dbEntity);
        long saveResult = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return saveResult > 0;
    }

    public Task<IList<D>> Find(Func<D, bool>? whereFilters = null)
    {
        Task<List<T>> results = (
            whereFilters != null ? _dbSet.Where(dbEntity => whereFilters(_mapper.Map<D>(dbEntity))) : _dbSet
        ).ToListAsync();

        return results.ContinueWith<IList<D>>(
            t => t.Result.Select(e => _mapper.Map<D>(e)).ToList(), 
            TaskContinuationOptions.ExecuteSynchronously
        );
    }

    public Task<D?> FindSingle(Func<D, bool> whereFilters)
    {
        return _dbSet.FirstOrDefaultAsync(e => whereFilters(_mapper.Map<D>(e)))
            .AvoidTracking(_dbSet)
            .ContinueWith(t => _mapper.Map<D?>(t.Result), TaskContinuationOptions.ExecuteSynchronously);
    }

    public Task<D?> GetById(long id)
    {
        return _dbSet.FindAsync(id).AsTask().AvoidTracking(_dbSet)
            .ContinueWith(t => _mapper.Map<D?>(t.Result), TaskContinuationOptions.ExecuteSynchronously);
    }

    public async Task<bool> Update(D entity)
    {
        _dbSet.Update(_mapper.Map<T>(entity));
        long saveResult = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return saveResult > 0;
    }
}
