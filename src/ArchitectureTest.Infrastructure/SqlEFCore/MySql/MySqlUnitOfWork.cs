using ArchitectureTest.Databases.MySql;
using ArchitectureTest.Databases.MySql.Entities;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;

namespace ArchitectureTest.Infrastructure.SqlEFCore.MySql;

public class MySqlUnitOfWork : IDomainUnitOfWork, IDisposable {
    private readonly DatabaseContext _databaseContext;
    private IDbContextTransaction? _transaction;
    private readonly Dictionary<string, object> _repos = new();
    private readonly IMapper _mapper;

    public MySqlUnitOfWork(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }

    public IDomainRepository<D> Repository<D>() where D : BaseEntity<long> {
        string typeName = typeof(D).Name;
        bool found = _repos.TryGetValue(typeName, out var repo);
        if (!found) {
            repo = CreateRepo<D>() ?? throw new Exception(ErrorCodes.RepoProblem);
            _repos.Add(typeName, repo);
        }
        return repo as IDomainRepository<D> ?? throw new Exception(ErrorCodes.RepoProblem);
    }

    public IDomainRepository<D>? CreateRepo<D>() where D : BaseEntity<long> {
        string typeName = typeof(D).Name;

        return typeName switch
        {
            var domainType when domainType == typeof(ChecklistEntity).Name => 
                new ChecklistMySqlRepository(_databaseContext, _mapper) as IDomainRepository<D>,
            var domainType when domainType == typeof(ChecklistDetailEntity).Name =>
                new SqlRepository<ChecklistDetailEntity, ChecklistDetail>(_databaseContext, _mapper) as IDomainRepository<D>,
            var domainType when domainType == typeof(NoteEntity).Name =>
                new SqlRepository<NoteEntity, Note>(_databaseContext, _mapper) as IDomainRepository<D>,
            _ => throw new NotImplementedException(ErrorCodes.RepoProblem)
        };
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
