using System.Collections.Generic;

namespace ArchitectureTest.Domain.Entities;

public class ChecklistDetail : BaseEntity<string>
{
    public string ChecklistId { get; set; } = string.Empty;
    public string? ParentDetailId { get; set; }
    public string? TaskName { get; set; }
    public bool Status { get; set; }
    public IList<ChecklistDetail>? SubItems { get; set; }
}
