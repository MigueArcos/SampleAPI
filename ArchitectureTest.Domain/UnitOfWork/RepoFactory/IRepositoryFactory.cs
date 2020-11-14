using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Repositories.BasicRepo;

namespace ArchitectureTest.Domain.UnitOfWork.RepoFactory {
	public interface IRepositoryFactory {
		IRepository<TEntity> Create<TEntity>() where TEntity : Entity;
	}
}
