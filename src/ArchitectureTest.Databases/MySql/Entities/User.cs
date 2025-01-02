using System;
using System.Collections.Generic;

namespace ArchitectureTest.Databases.MySql.Entities;

public partial class User
{
    public string Id { get; set; }

    public string Email { get; set; }

    public string Name { get; set; }

    public string Password { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? ModificationDate { get; set; }

    public virtual ICollection<Checklist> Checklists { get; set; } = new List<Checklist>();

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}
