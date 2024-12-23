using System;
using System.Collections.Generic;
using System.Linq;

namespace ArchitectureTest.Domain.Entities;

public class ChecklistEntity : BaseEntity<long> {
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
    public IList<ChecklistDetailEntity>? Details { get; set; }

    public static IList<ChecklistDetailEntity>? FormatChecklistDetails(
        ICollection<ChecklistDetailEntity>? details, long? parentDetailId = null
    ){
        var selection = details?.Where(d => d.ParentDetailId == parentDetailId).Select(cD => new ChecklistDetailEntity {
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
