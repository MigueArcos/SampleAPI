﻿using System;

namespace ArchitectureTest.Domain.Entities;

public class UserTokenEntity : BaseEntity<long>
{
    public long UserId { get; set; }

    public long TokenTypeId { get; set; }

    public required string Token { get; set; }

    public DateTime? ExpiryTime { get; set; }
}
