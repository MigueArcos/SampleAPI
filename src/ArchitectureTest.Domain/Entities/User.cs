namespace ArchitectureTest.Domain.Entities;

public class User : BaseEntity<string>
{
    public required string Email { get; set; }

    public required string Password { get; set; }

    public string? Name { get; set; }
}
