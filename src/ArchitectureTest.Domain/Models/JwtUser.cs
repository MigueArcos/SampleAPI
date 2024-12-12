namespace ArchitectureTest.Domain.Models; 

public class JwtUser {
	public long Id { get; set; }
	public required string Name { get; set; }
	public required string Email { get; set; }
}
