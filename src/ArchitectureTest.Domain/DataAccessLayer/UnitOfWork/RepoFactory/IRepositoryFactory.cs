using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;

namespace ArchitectureTest.Domain.DataAccessLayer.UnitOfWork.RepoFactory; 
public interface IRepositoryFactory {
    IRepository<long, TEntity>? Create<TEntity>() where TEntity : class;
}
