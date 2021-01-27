using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.DataAccessLayer.Repositories;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;

namespace ArchitectureTest.Domain.DataAccessLayer.UnitOfWork.RepoFactory {
	public class RepositoryFactory : IRepositoryFactory {
		private DatabaseContext databaseContext;

		public RepositoryFactory(DatabaseContext databaseContext) {
			this.databaseContext = databaseContext;
		}

		public IRepository<TEntity> Create<TEntity>() where TEntity : class {
			string typeName = typeof(TEntity).Name;
            if (typeName == typeof(Checklist).Name) {
                return new ChecklistRepository(databaseContext) as IRepository<TEntity>;
            }
            else {
                return new Repository<TEntity>(databaseContext);
            }
		}
	}
}
