namespace ArchitectureTest.Domain.Services;

public interface IRepositoryFactory {
    IRepository<long, TEntity>? Create<TEntity>() where TEntity : class;
}
