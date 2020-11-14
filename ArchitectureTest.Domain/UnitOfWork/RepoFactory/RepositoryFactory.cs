using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Repositories;
using ArchitectureTest.Domain.Repositories.BasicRepo;

namespace ArchitectureTest.Domain.UnitOfWork.RepoFactory {
	public class RepositoryFactory : IRepositoryFactory {
		private DatabaseContext databaseContext;

		public RepositoryFactory(DatabaseContext databaseContext) {
			this.databaseContext = databaseContext;
		}

		public IRepository<TEntity> Create<TEntity>() where TEntity : Entity {
			string typeName = typeof(TEntity).Name;
			switch (typeName) {
				case "Checklist":
					return new ChecklistRepository(databaseContext) as IRepository<TEntity>;
				default:
					return new Repository<TEntity>(databaseContext);
			}
		}
	}
}
