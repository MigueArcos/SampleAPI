using System;

namespace ArchitectureTest.Domain.Models;

public record NoteDTO
{
    public string? Id { get; init; }
    public string? UserId { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public DateTime? CreationDate { get; init; }
    public DateTime? ModificationDate { get; init; }
}
