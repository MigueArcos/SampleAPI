using ArchitectureTest.Data.Database.SQLServer;
using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.DataAccessLayer.Repositories;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;

namespace ArchitectureTest.Domain.DataAccessLayer.UnitOfWork.RepoFactory; 
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
