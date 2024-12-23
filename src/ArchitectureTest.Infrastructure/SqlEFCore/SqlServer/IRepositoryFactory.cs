
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;

public interface IRepositoryFactory {
    IRepository<D>? Create<D>() where D : BaseEntity<long>;
}
