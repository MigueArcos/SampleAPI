using System;
using System.Collections.Generic;

namespace ArchitectureTest.Data.Database.SQLServer.Entities;

public partial class Note
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? ModificationDate { get; set; }

    public virtual User User { get; set; }
}
