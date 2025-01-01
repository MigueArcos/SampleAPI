namespace ArchitectureTest.Domain.Models.Application;

public class UserTokenIdentity {
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public string? Name { get; set; }
}
