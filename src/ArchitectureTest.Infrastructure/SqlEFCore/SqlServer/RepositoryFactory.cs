using System;
using ArchitectureTest.Databases.SqlServer;
using ArchitectureTest.Domain.Services;
using AutoMapper;
using ArchitectureTest.Infrastructure.SqlEFCore.Common;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;

public class SqlServerRepositoryFactory : BaseRepositoryFactory {
    private readonly DatabaseContext _databaseContext;
    private const string _entitiesNamespace = "SqlServer.Entities";

    public SqlServerRepositoryFactory(DatabaseContext dbContext, IMapper mapper) : base(dbContext, mapper, _entitiesNamespace)
    {
        _databaseContext = dbContext;
    }

    public override IRepository<D>? Create<D>() {
        Type entityType = typeof(D);

        return entityType.Name switch
        {
            var domainTypeName when domainTypeName == typeof(Domain.Entities.Checklist).Name =>
                new SqlServerChecklistRepository(_databaseContext, _mapper) as IRepository<D>,
            /* [OLD] This is another way to build the GenericRepository without refelection
            var domainType when domainType == typeof(DomainEntities.UserToken).Name =>
                new SqlRepository<DomainEntities.UserToken, Database.UserToken>(_databaseContext, _mapper) as IRepository<D>,    
            */
            _ => base.Create<D>()
        };
    }
}
