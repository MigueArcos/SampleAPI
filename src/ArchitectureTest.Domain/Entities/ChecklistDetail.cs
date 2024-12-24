using System;
using System.Collections.Generic;

namespace ArchitectureTest.Domain.Entities; 

public class ChecklistDetail : BaseEntity<long> {
    public long ChecklistId { get; set; }
    public long? ParentDetailId { get; set; }
    public string? TaskName { get; set; }
    public bool Status { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
    public IList<ChecklistDetail>? SubItems { get; set; }
}
