using System;
using ArchitectureTest.Databases.SqlServer;
using ArchitectureTest.Databases.SqlServer.Entities;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Services;
using AutoMapper;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;
 
public class RepositoryFactory : IRepositoryFactory {
    private readonly DatabaseContext _databaseContext;
    private readonly IMapper _mapper;

    public RepositoryFactory(DatabaseContext databaseContext, IMapper mapper)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
    }

    public IDomainRepository<D>? Create<D>() where D : BaseEntity<long> {
        string typeName = typeof(D).Name;

        return typeName switch
        {
            var domainType when domainType == typeof(ChecklistEntity).Name => 
                new ChecklistSqlServerRepository(_databaseContext, _mapper) as IDomainRepository<D>,
            var domainType when domainType == typeof(ChecklistDetailEntity).Name => 
                new SqlRepository<ChecklistDetailEntity, ChecklistDetail>(_databaseContext, _mapper) as IDomainRepository<D>,
            var domainType when domainType == typeof(NoteEntity).Name =>
                new SqlRepository<NoteEntity, Note>(_databaseContext, _mapper) as IDomainRepository<D>,
            _ => throw new NotImplementedException(ErrorCodes.RepoProblem),
        };
    }
}
