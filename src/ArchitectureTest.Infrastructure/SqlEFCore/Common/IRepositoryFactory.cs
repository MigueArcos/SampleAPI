
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services;

namespace ArchitectureTest.Infrastructure.SqlEFCore.Common;

public interface IRepositoryFactory {
    IRepository<D>? Create<D>() where D : BaseEntity<string>;
}
