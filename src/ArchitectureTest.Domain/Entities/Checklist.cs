using System;
using System.Collections.Generic;
using System.Linq;

namespace ArchitectureTest.Domain.Entities;

public class Checklist : BaseEntity<long> {
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
    public IList<ChecklistDetail>? Details { get; set; }

    public static IList<ChecklistDetail>? FormatChecklistDetails(
        ICollection<ChecklistDetail>? details, long? parentDetailId = null
    ){
        var selection = details?.Where(d => d.ParentDetailId == parentDetailId).Select(cD => new ChecklistDetail {
            Id = cD.Id,
            ChecklistId = cD.ChecklistId,
            ParentDetailId = cD.ParentDetailId,
            TaskName = cD.TaskName,
            Status = cD.Status,
            CreationDate = cD.CreationDate,
            ModificationDate = cD.ModificationDate
        }).ToList();
        selection?.ForEach(i => {
            i.SubItems = FormatChecklistDetails(details, i.Id);
        });
        return selection;
    }
}
