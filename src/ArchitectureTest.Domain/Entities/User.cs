using System;

namespace ArchitectureTest.Domain.Entities;

public class UserEntity : BaseEntity<long>
{
    public required string Email { get; set; }

    public string? Name { get; set; }

    public required string Password { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? ModificationDate { get; set; }
}
