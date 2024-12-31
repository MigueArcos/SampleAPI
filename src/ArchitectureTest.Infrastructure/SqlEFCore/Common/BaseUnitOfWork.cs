using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Infrastructure.SqlEFCore.Common;

public class BaseUnitOfWork : IUnitOfWork, IDisposable {
    private readonly DbContext _databaseContext;
    private IDbContextTransaction? _transaction;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly Dictionary<string, object> _repos = new();

    public BaseUnitOfWork(DbContext dbContext, IRepositoryFactory repositoryFactory)
    {
        _databaseContext = dbContext;
        _repositoryFactory = repositoryFactory;
    }

    public IRepository<D> Repository<D>() where D : BaseEntity<string> {
        string typeName = typeof(D).Name;
        bool found = _repos.TryGetValue(typeName, out var repo);
        if (!found) {
            repo = _repositoryFactory.Create<D>() ?? throw new Exception(ErrorCodes.RepoProblem);
            _repos.Add(typeName, repo);
        }
        return repo as IRepository<D> ?? throw new Exception(ErrorCodes.RepoProblem);
    }

    public async Task Commit() {
        try {
            await SaveChanges().ConfigureAwait(false);
            await _transaction!.CommitAsync().ConfigureAwait(false);
        }
        finally {
            _transaction!.Dispose();
        }
    }

    public async Task Rollback() {
        await _transaction!.RollbackAsync().ConfigureAwait(false);
        _transaction!.Dispose();
    }

    public async Task StartTransaction() {
        _transaction = await _databaseContext.Database.BeginTransactionAsync().ConfigureAwait(false);
    }


    private bool _disposed = false;

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                _databaseContext.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Task<int> SaveChanges()
    {
        return _databaseContext.SaveChangesAsync();
    }
}
