using System;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArchitectureTest.Infrastructure.SqlEFCore.Common;
 
public abstract class BaseRepositoryFactory : IRepositoryFactory {
    private readonly DbContext _databaseContext;
    protected readonly IMapper _mapper;
    private readonly string _entitiesNamespace;

    public BaseRepositoryFactory(DbContext databaseContext, IMapper mapper, string entitiesNamespace)
    {
        _databaseContext = databaseContext;
        _mapper = mapper;
        _entitiesNamespace = entitiesNamespace;
    }

    public virtual IRepository<D>? Create<D>() where D : BaseEntity<long> {
        Type entityType = typeof(D);
        return BuildGenericRepo<D>(entityType);
    }

    private IRepository<D>? BuildGenericRepo<D>(Type domainType) where D : BaseEntity<long> {
        var dbAssemblyName = "ArchitectureTest.Databases";
        var genericRepoType = typeof(SqlRepository<,>);
        Type[] typeArgs = [
            typeof(D),
            Type.GetType($"{dbAssemblyName}.{_entitiesNamespace}.{domainType.Name}, {dbAssemblyName}")!
        ];
        var genericRepoTypeWithArgs = genericRepoType.MakeGenericType(typeArgs);
        object? rawRepo = Activator.CreateInstance(genericRepoTypeWithArgs, _databaseContext, _mapper);
        return rawRepo as IRepository<D>;
    }
}
