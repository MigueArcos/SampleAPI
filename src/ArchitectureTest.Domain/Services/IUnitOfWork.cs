namespace ArchitectureTest.Domain.Services; 

public interface IUnitOfWork {
    IRepository<long, TEntity> Repository<TEntity>() where TEntity : class;
    void StartTransaction();
    void Commit();
    void Rollback();
}
