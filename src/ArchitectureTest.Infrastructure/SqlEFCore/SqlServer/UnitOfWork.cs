using ArchitectureTest.Databases.SqlServer;
using ArchitectureTest.Infrastructure.SqlEFCore.Common;
using AutoMapper;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;

public class SqlSeverUnitOfWork : BaseUnitOfWork {

    public SqlSeverUnitOfWork(
        DatabaseContext dbContext, IMapper mapper
    ) : base(dbContext, new SqlServerRepositoryFactory(dbContext, mapper)){ }
}
