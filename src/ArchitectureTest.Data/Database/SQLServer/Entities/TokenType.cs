﻿using System;
using System.Collections.Generic;

namespace ArchitectureTest.Data.Database.SQLServer.Entities;

public partial class TokenType
{
    public long Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}
