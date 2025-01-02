namespace ArchitectureTest.Domain.Entities;

public class Note : BaseEntity<string>
{
    public required string UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
}
