using ArchitectureTest.Databases.MySql;
using AutoMapper;
using Database = ArchitectureTest.Databases.MySql.Entities;
using ArchitectureTest.Infrastructure.SqlEFCore.Common;
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace ArchitectureTest.Infrastructure.SqlEFCore.MySql;

public class MySqlChecklistRepository : BaseChecklistRepository<Database.Checklist>
{
    private readonly DbSet<Database.ChecklistDetail> _checklistDetailsDbSet;
    private readonly static Func<string, Expression<Func<Database.Checklist, bool>>> findByIdExpr = 
        id => checklist => checklist.Id == id;

    public MySqlChecklistRepository(DatabaseContext dbContext, IMapper mapper) : base(dbContext, mapper, findByIdExpr) {
        _checklistDetailsDbSet = dbContext.Set<Database.ChecklistDetail>();
    }

    public async Task<int> DeleteDetails(string checklistId, bool autoSave = true)
    {
        int deleteCount = await _checklistDetailsDbSet.Where(d => d.ChecklistId == checklistId).ExecuteDeleteAsync()
            .ConfigureAwait(false);
        
        // if (autoSave)
        //     await _db
        return deleteCount;
    }
}
