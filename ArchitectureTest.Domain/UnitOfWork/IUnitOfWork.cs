using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Repositories.BasicRepo;

namespace ArchitectureTest.Domain.UnitOfWork {
	public interface IUnitOfWork {
		IRepository<TEntity> Repository<TEntity>() where TEntity : class;
		void StartTransaction();
		void Commit();
		void Rollback();
	}
}
