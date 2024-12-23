using System;
using ArchitectureTest.Databases.SqlServer;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services;
using AutoMapper;

using DomainEntities = ArchitectureTest.Domain.Entities;
using Database = ArchitectureTest.Databases.SqlServer.Entities;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;
 
public class RepositoryFactory : IRepositoryFactory {
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public RepositoryFactory(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }

    public IRepository<D>? Create<D>() where D : BaseEntity<long> {
        string typeName = typeof(D).Name;

        // If there were only one type Argument (D type) I could create the SqlRepository without this ugly switch
        // TODO: Maybe this can be changed using reflection to avoid this boilerplate code for all the common repos
        // The only "special" repository is the ChecklistSqlServerRepository
        return typeName switch
        {
            var domainType when domainType == typeof(Domain.Entities.Checklist).Name =>
                new ChecklistSqlServerRepository(_databaseContext, _mapper) as IRepository<D>,
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
}
