using System;

namespace ArchitectureTest.Domain.Entities; 

public class Note : BaseEntity<long> {
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
}
