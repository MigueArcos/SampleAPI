using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;

namespace ArchitectureTest.Domain.DataAccessLayer.UnitOfWork; 
public interface IUnitOfWork {
	IRepository<TEntity> Repository<TEntity>() where TEntity : class;
	void StartTransaction();
	void Commit();
	void Rollback();
}
