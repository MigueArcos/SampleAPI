using ArchitectureTest.Domain.Entities;

namespace ArchitectureTest.Domain.Services; 

public interface IDomainUnitOfWork {
    IDomainRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity<long>;
    void StartTransaction();
    void Commit();
    void Rollback();
}
