using System;
using System.Collections.Generic;

namespace ArchitectureTest.Databases.SqlServer.Entities;

public partial class UserToken
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public string TokenTypeId { get; set; }

    public string Token { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? ExpiryTime { get; set; }

    public virtual TokenType TokenType { get; set; }

    public virtual User User { get; set; }
}
