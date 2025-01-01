using System;
using System.Collections.Generic;

namespace ArchitectureTest.Databases.SqlServer.Entities;

public partial class Checklist
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public string Title { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? ModificationDate { get; set; }

    public virtual ICollection<ChecklistDetail> ChecklistDetails { get; set; } = new List<ChecklistDetail>();

    public virtual User User { get; set; }
}
