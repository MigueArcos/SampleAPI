﻿using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;

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

    public IRepository<D> Repository<D>() where D : BaseEntity<long> {
        string typeName = typeof(D).Name;
        bool found = _repos.TryGetValue(typeName, out var repo);
        if (!found) {
            repo = _repositoryFactory.Create<D>() ?? throw new Exception(ErrorCodes.RepoProblem);
            _repos.Add(typeName, repo);
        }
        return repo as IRepository<D> ?? throw new Exception(ErrorCodes.RepoProblem);
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