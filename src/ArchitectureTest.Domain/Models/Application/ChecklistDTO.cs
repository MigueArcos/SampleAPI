using System;
using System.Collections.Generic;

namespace ArchitectureTest.Domain.Models;

public record ChecklistDTO
{
    public string? Id { get; init; }
    public string? UserId { get; init; }
    public string? Title { get; init; }
    public IList<ChecklistDetailDTO>? Details { get; init; }
    public DateTime? CreationDate { get; init; }
    public DateTime? ModificationDat { get; init; }
}

public record ChecklistDetailDTO
{
    public string? Id { get; init; }
    public string? ChecklistId { get; init; }
    public string? ParentDetailId { get; init; }
    public string? TaskName { get; init; }
    public bool Status { get; init; }
    public IList<ChecklistDetailDTO>? SubItems { get; set; }
    public DateTime? CreationDate { get; init; }
    public DateTime? ModificationDate { get; init; }
}

public record UpdateChecklistDTO : ChecklistDTO
{
    IList<ChecklistDetailDTO>? DetailsToAdd { get; init; }
    IList<ChecklistDetailDTO>? DetailsToUpdate { get; init; }
    IList<string>? DetailsToDelete { get; init; }
}
