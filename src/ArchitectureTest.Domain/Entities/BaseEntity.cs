using System;

namespace ArchitectureTest.Domain.Entities;

public class BaseEntity<K> {
    public required K Id { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? ModificationDate { get; set; }
}
