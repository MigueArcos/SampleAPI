using System;
using System.Collections.Generic;
using System.Linq;

namespace ArchitectureTest.Domain.Models;

public record ChecklistDTO
{
    public string? Id { get; init; }
    public string? UserId { get; init; }
    public string? Title { get; init; }
    public List<ChecklistDetailDTO>? Details { get; init; }
    public DateTime? CreationDate { get; init; }
    public DateTime? ModificationDate { get; init; }
}

public record ChecklistDetailDTO
{
    public string? Id { get; init; }
    public string? ChecklistId { get; init; }
    public string? ParentDetailId { get; init; }
    public string? TaskName { get; init; }
    public bool Status { get; init; }
    public List<ChecklistDetailDTO>? SubItems { get; set; }
    public DateTime? CreationDate { get; init; }
    public DateTime? ModificationDate { get; init; }
}

public record UpdateChecklistDTO : ChecklistDTO
{
    public UpdateChecklistDTO()
    {
        Details = [];
    }

    public List<ChecklistDetailDTO>? ProcessDetailsToUpdate(List<string>? allDetailsToDelete)
    {
        // Remove all details without ID and also set the SubItems of these details to empty []
        var detailsToUpdateAsDictionary = DetailsToUpdate?
            .Where(d => !string.IsNullOrWhiteSpace(d.Id))
            .ToDictionary(d => d.Id!, d => d with { SubItems = [] }) ?? [];

        allDetailsToDelete?.ForEach(id => {
            detailsToUpdateAsDictionary.Remove(id);
        });

        return detailsToUpdateAsDictionary?.Select(kvp => kvp.Value).ToList();
    }

    public List<ChecklistDetailDTO>? DetailsToAdd { get; init; }

    // These Details will be "plain details", this means that its properties SubItems will be ignored, why?
    // This is because at this point we already know its IDs, so we expect all DetailsToUpdate to have its IDs already
    // populated, if we would use SubItems for these Detail we would need to use a complex logic to move the ParentDetailId
    // accordingly to the corresponding parent, although possible, this is cumbersome and can lead to possible data problems
    // e.g: a subitem having a ParentDetailId not corresponding to its actual parent in the payload, what is the source of truth?
    // we don't know if the user actually wants to use the ParentDetailId provided in the object or if it only wants to move it
    // to its corresponding parent in the tree
    public List<ChecklistDetailDTO>? DetailsToUpdate { get; init; }

    public List<string>? DetailsToDelete { get; init; }
}
