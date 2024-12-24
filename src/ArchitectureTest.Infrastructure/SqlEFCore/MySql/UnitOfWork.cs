using ArchitectureTest.Databases.MySql;
using ArchitectureTest.Infrastructure.SqlEFCore.Common;
using AutoMapper;

namespace ArchitectureTest.Infrastructure.SqlEFCore.MySql;

public class MySqlUnitOfWork : BaseUnitOfWork {

    public MySqlUnitOfWork(
        DatabaseContext dbContext, IMapper mapper
    ) : base(dbContext, new MySqlRepositoryFactory(dbContext, mapper)){ }
}
