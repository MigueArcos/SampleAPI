using System;
using System.Collections.Generic;

namespace ArchitectureTest.Domain.Entities;

public class ChecklistEntity : BaseEntity<long>, IAggregator<ChecklistDetailEntity> {
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
    public IList<ChecklistDetailEntity>? Details { get; set; }

    public IList<ChecklistDetailEntity>? GetChildEntities()
    {
        return Details;
    }
}
