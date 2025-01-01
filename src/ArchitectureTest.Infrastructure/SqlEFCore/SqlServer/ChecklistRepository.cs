using ArchitectureTest.Databases.SqlServer;
using AutoMapper;
using System;
using System.Linq.Expressions;
using Database = ArchitectureTest.Databases.SqlServer.Entities;
using ArchitectureTest.Infrastructure.SqlEFCore.Common;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;

public class SqlServerChecklistRepository : BaseChecklistRepository<Database.Checklist>
{
    private readonly static Func<string, Expression<Func<Database.Checklist, bool>>> findByIdExpr = 
        id => checklist => checklist.Id == id;

    public SqlServerChecklistRepository(DatabaseContext dbContext, IMapper mapper) : base(dbContext, mapper, findByIdExpr) {}
}
