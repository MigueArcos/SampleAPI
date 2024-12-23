using ArchitectureTest.Domain.Entities;

namespace ArchitectureTest.Domain.Services; 

public interface IUnitOfWork {
    IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity<long>;
    void StartTransaction();
    void Commit();
    void Rollback();
}
