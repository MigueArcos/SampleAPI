﻿using System;
using System.Collections.Generic;

namespace ArchitectureTest.Databases.SqlServer.Entities;

public partial class ChecklistDetail
{
    public long Id { get; set; }

    public long ChecklistId { get; set; }

    public long? ParentDetailId { get; set; }

    public string TaskName { get; set; }

    public bool Status { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? ModificationDate { get; set; }

    public virtual Checklist Checklist { get; set; }
}
