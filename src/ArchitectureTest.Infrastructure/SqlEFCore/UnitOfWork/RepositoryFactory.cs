using ArchitectureTest.Databases.SqlServer;
using ArchitectureTest.Databases.SqlServer.Entities;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Infrastructure.SqlEFCore.Repostories;

namespace ArchitectureTest.Infrastructure.SqlEFCore.UnitOfWork;
 
public class RepositoryFactory : IRepositoryFactory {
    private readonly DatabaseContext _databaseContext;

    public RepositoryFactory(DatabaseContext databaseContext) {
        _databaseContext = databaseContext;
    }

    public IRepository<long, TEntity>? Create<TEntity>() where TEntity : class {
        string typeName = typeof(TEntity).Name;
        if (typeName == typeof(Checklist).Name) {
            return new ChecklistRepository(_databaseContext) as IRepository<long, TEntity>;
        }
        else {
            return new Repository<TEntity>(_databaseContext);
        }
    }
}
