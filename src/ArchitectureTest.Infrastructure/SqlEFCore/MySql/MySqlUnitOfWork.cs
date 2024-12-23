using ArchitectureTest.Databases.MySql;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;

using DomainEntities = ArchitectureTest.Domain.Entities;
using Database = ArchitectureTest.Databases.MySql.Entities;

namespace ArchitectureTest.Infrastructure.SqlEFCore.MySql;

public class MySqlUnitOfWork : IUnitOfWork, IDisposable {
    private readonly DatabaseContext _databaseContext;
    private IDbContextTransaction? _transaction;
    private readonly Dictionary<string, object> _repos = new();
    private readonly IMapper _mapper;

    public MySqlUnitOfWork(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }

    public IRepository<D> Repository<D>() where D : BaseEntity<long> {
        string typeName = typeof(D).Name;
        bool found = _repos.TryGetValue(typeName, out var repo);
        if (!found) {
            repo = CreateRepo<D>() ?? throw new Exception(ErrorCodes.RepoProblem);
            _repos.Add(typeName, repo);
        }
        return repo as IRepository<D> ?? throw new Exception(ErrorCodes.RepoProblem);
    }

    public IRepository<D>? CreateRepo<D>() where D : BaseEntity<long> {
        string typeName = typeof(D).Name;

        // If there were only one type Argument (D type) I could create the SqlRepository without this ugly switch
        // TODO: Maybe this can be changed using reflection to avoid this boilerplate code for all the common repos
        // The only "special" repository is the ChecklistSqlServerRepository
        return typeName switch
        {
            var domainType when domainType == typeof(DomainEntities.Checklist).Name =>
                new ChecklistMySqlRepository(_databaseContext, _mapper) as IRepository<D>,
            var domainType when domainType == typeof(DomainEntities.ChecklistDetail).Name =>
                new SqlRepository<DomainEntities.ChecklistDetail, Database.ChecklistDetail>(_databaseContext, _mapper)
                    as IRepository<D>,
            var domainType when domainType == typeof(DomainEntities.Note).Name =>
                new SqlRepository<DomainEntities.Note, Database.Note>(_databaseContext, _mapper) as IRepository<D>,
            var domainType when domainType == typeof(DomainEntities.User).Name =>
                new SqlRepository<DomainEntities.User, Database.User>(_databaseContext, _mapper) as IRepository<D>,
            var domainType when domainType == typeof(DomainEntities.UserToken).Name =>
                new SqlRepository<DomainEntities.UserToken, Database.UserToken>(_databaseContext, _mapper) as IRepository<D>,
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
