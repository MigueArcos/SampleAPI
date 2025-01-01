using System.Collections.Generic;
using System.Linq;

namespace ArchitectureTest.Domain.Entities;

public class Checklist : BaseEntity<string> {
    public required string UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public IList<ChecklistDetail>? Details { get; set; }

    public static List<ChecklistDetail>? FormatChecklistDetails(
        ICollection<ChecklistDetail>? details, string? parentDetailId = null
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
