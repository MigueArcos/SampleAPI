using System;
using System.Collections.Generic;

namespace ArchitectureTest.Databases.SqlServer.Entities;

public partial class TokenType
{
    public string Id { get; set; }

    public string Name { get; set; }

    public DateTime CreationDate { get; set; }

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}
