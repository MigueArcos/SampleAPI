using System;
using System.Collections.Generic;

namespace ArchitectureTest.Databases.MySql.Entities;

public partial class ChecklistDetail
{
    public string Id { get; set; }

    public string ChecklistId { get; set; }

    public string ParentDetailId { get; set; }

    public string TaskName { get; set; }

    public bool Status { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? ModificationDate { get; set; }

    public virtual Checklist Checklist { get; set; }
}
