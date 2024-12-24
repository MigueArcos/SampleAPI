namespace ArchitectureTest.Domain.Models.Application;

public class UserTokenIdentity {
    public long UserId { get; set; }
    public string? Name { get; set; }
    public required string Email { get; set; }
}
