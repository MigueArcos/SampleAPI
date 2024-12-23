using System;
using ArchitectureTest.Domain.Services;
using AutoMapper;
using ArchitectureTest.Infrastructure.SqlEFCore.Common;
using ArchitectureTest.Databases.MySql;

namespace ArchitectureTest.Infrastructure.SqlEFCore.MySql;

public class MySqlRepositoryFactory : BaseRepositoryFactory {
    private readonly DatabaseContext _databaseContext;
    private const string _entitiesNamespace = "MySql.Entities";

    public MySqlRepositoryFactory(DatabaseContext dbContext, IMapper mapper) : base(dbContext, mapper, _entitiesNamespace)
    {
        _databaseContext = dbContext;
    }

    public override IRepository<D>? Create<D>() {
        Type entityType = typeof(D);

        return entityType.Name switch
        {
            var domainTypeName when domainTypeName == typeof(Domain.Entities.Checklist).Name =>
                new MySqlChecklistRepository(_databaseContext, _mapper) as IRepository<D>,
            _ => base.Create<D>()
        };
    }
}
