namespace ArchitectureTest.Domain.Models;

public class JsonWebToken {
    public required long UserId { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public required long ExpiresIn { get; set; }
}
