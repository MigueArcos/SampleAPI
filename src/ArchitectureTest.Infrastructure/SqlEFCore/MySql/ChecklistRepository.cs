using ArchitectureTest.Databases.MySql;
using AutoMapper;
using Database = ArchitectureTest.Databases.MySql.Entities;
using ArchitectureTest.Infrastructure.SqlEFCore.Common;
using System;
using System.Linq.Expressions;

namespace ArchitectureTest.Infrastructure.SqlEFCore.MySql;

public class MySqlChecklistRepository : BaseChecklistRepository<Database.Checklist>
{
    private readonly static Func<long, Expression<Func<Database.Checklist, bool>>> findByIdExpr = 
        id => checklist => checklist.Id == id;

    public MySqlChecklistRepository(DatabaseContext dbContext, IMapper mapper) : base(dbContext, mapper, findByIdExpr) {}
}
