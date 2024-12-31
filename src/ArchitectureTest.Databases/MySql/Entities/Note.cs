using System;
using System.Collections.Generic;

namespace ArchitectureTest.Databases.MySql.Entities;

public partial class Note
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? ModificationDate { get; set; }

    public virtual User User { get; set; }
}
