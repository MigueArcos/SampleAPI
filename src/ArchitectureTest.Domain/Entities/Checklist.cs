using System.Collections.Generic;

namespace ArchitectureTest.Domain.Entities;

public class Checklist : BaseEntity<string> {
    public required string UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public IList<ChecklistDetail>? Details { get; set; }
}
