using ArchitectureTest.Databases.MySql;
using AutoMapper;
using Database = ArchitectureTest.Databases.MySql.Entities;
using ArchitectureTest.Infrastructure.SqlEFCore.Common;
using System;
using System.Linq.Expressions;

namespace ArchitectureTest.Infrastructure.SqlEFCore.MySql;

public class MySqlChecklistRepository : BaseChecklistRepository<Database.Checklist, Database.ChecklistDetail>
{
    public MySqlChecklistRepository(DatabaseContext dbContext, IMapper mapper) : base(dbContext, mapper) {}

    private readonly Func<string, Expression<Func<Database.Checklist, bool>>> findByIdExpr = 
        id => checklist => checklist.Id == id;

    private readonly Func<string, Expression<Func<Database.ChecklistDetail, bool>>> findDetailByChecklistIdExpr = 
        checklistId => checklistDetail => checklistDetail.ChecklistId == checklistId;

    public override Expression<Func<Database.Checklist, bool>> BuildFindByIdPredicate(string id) =>
        findByIdExpr(id);

    public override Expression<Func<Database.ChecklistDetail, bool>> BuildFindDetailByChecklistIdPredicate(string checklistId) =>
        findDetailByChecklistIdExpr(checklistId);
}
