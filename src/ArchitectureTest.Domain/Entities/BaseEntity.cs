using System;

namespace ArchitectureTest.Domain.Entities;

public class BaseEntity<K> {
    public K Id { get; set; } = default!;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? ModificationDate { get; set; }
}
