using System;

namespace ArchitectureTest.Domain.Entities;

public class UserToken : BaseEntity<string>
{
    public required string UserId { get; set; }

    public required string TokenTypeId { get; set; }

    public required string Token { get; set; }

    public DateTime? ExpiryTime { get; set; }
}
