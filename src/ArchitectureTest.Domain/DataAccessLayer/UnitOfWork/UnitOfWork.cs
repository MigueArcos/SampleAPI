using ArchitectureTest.Data.Database.SQLServer;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork.RepoFactory;
using ArchitectureTest.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;

namespace ArchitectureTest.Domain.DataAccessLayer.UnitOfWork; 
public class UnitOfWork : IUnitOfWork, IDisposable {
    private readonly DatabaseContext _databaseContext;
    private IDbContextTransaction? _transaction;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly Dictionary<string, object> _repos = new();

    public UnitOfWork(DatabaseContext databaseContext) {
        _databaseContext = databaseContext;
        _repositoryFactory = new RepositoryFactory(databaseContext);
    }

    public IRepository<long, TEntity> Repository<TEntity>() where TEntity : class {
        string typeName = typeof(TEntity).Name;
        bool found = _repos.TryGetValue(typeName, out var repo);
        if (!found) {
            repo = _repositoryFactory.Create<TEntity>() ?? throw new Exception(ErrorCodes.RepoProblem);
            _repos.Add(typeName, repo);
        }
        return repo as IRepository<long, TEntity> ?? throw new Exception(ErrorCodes.RepoProblem);
    }

    public void Commit() {
        try {
            _databaseContext.SaveChanges();
            _transaction!.Commit();
        }
        finally {
            _transaction!.Dispose();
        }
    }

    public void Rollback() {
        _transaction!.Rollback();
        _transaction!.Dispose();
    }

    public void StartTransaction() {
        _transaction = _databaseContext.Database.BeginTransaction();
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
}
