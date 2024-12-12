namespace ArchitectureTest.Domain.Models;

public class JsonWebToken {
	public long UserId { get; set; }
	public required string Email { get; set; }
	public required string Token { get; set; }
	public required string RefreshToken { get; set; }
	public long ExpiresIn { get; set; }
}
