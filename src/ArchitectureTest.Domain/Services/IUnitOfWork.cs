using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;

namespace ArchitectureTest.Domain.Services;

public interface IUnitOfWork {
    IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity<string>;
    Task StartTransaction();
    Task Commit();
    Task Rollback();
    Task<int> SaveChanges();
}
