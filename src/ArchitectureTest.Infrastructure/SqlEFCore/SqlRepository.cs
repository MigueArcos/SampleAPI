using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Infrastructure.ExpressionUtils;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArchitectureTest.Infrastructure.SqlEFCore;

public class SqlRepository<D, T> : IRepository<D>
    where D : BaseEntity<long> // Domain Entity
    where T : class            // Database Table Entity
{
    private readonly DbContext _dbContext;
    protected readonly IMapper _mapper;
    protected readonly DbSet<T> _dbSet;

    public SqlRepository(DbContext dbContext, IMapper mapper)
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
        return _mapper.Map<D>(dbEntity);
    }

    public async Task<bool> DeleteById(long id)
    {
        T? dbEntity = await _dbSet.FindAsync(id).ConfigureAwait(false)
            ?? throw new Exception(ErrorCodes.EntityNotFound);
        
        _dbSet.Remove(dbEntity);
        long saveResult = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return saveResult > 0;
    }

    public virtual Task<IList<D>> Find(Expression<Func<D, bool>>? whereFilters = null)
    {
        Task<List<T>> results = (
            whereFilters != null ? _dbSet.Where(whereFilters.ReplaceLambdaParameter<D, T>()) : _dbSet
        ).ToListAsync();

        return results.ContinueWith<IList<D>>(
            t => t.Result?.Select(e => _mapper.Map<D>(e)).ToList() ?? [],
            TaskContinuationOptions.ExecuteSynchronously
        );
    }

    public virtual Task<D?> FindSingle(Expression<Func<D, bool>> whereFilters)
    {
        var whereForDatabase = whereFilters.ReplaceLambdaParameter<D, T>();
        return _dbSet.FirstOrDefaultAsync(whereForDatabase)
            .AvoidTracking(_dbSet)
            .ContinueWith(t => _mapper.Map<D?>(t.Result), TaskContinuationOptions.ExecuteSynchronously);
    }

    public virtual Task<D?> GetById(long id)
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
