using ArchitectureTest.Databases.SqlServer;
using AutoMapper;
using System;
using System.Linq.Expressions;
using Database = ArchitectureTest.Databases.SqlServer.Entities;
using ArchitectureTest.Infrastructure.SqlEFCore.Common;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;

public class SqlServerChecklistRepository : BaseChecklistRepository<Database.Checklist, Database.ChecklistDetail>
{
    public SqlServerChecklistRepository(DatabaseContext dbContext, IMapper mapper) : base(dbContext, mapper) {}

    public override Expression<Func<Database.Checklist, bool>> BuildFindByIdPredicate(string id) =>
        checklist => checklist.Id == id;

    public override Expression<Func<Database.ChecklistDetail, bool>> BuildFindDetailByChecklistIdPredicate(string checklistId) =>
        checklistDetail => checklistDetail.ChecklistId == checklistId;
}
