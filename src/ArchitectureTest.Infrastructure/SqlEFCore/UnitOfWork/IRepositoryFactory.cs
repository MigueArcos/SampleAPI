using ArchitectureTest.Domain.Services;

namespace ArchitectureTest.Infrastructure.SqlEFCore.UnitOfWork;

public interface IRepositoryFactory {
    IRepository<long, TEntity>? Create<TEntity>() where TEntity : class;
}
