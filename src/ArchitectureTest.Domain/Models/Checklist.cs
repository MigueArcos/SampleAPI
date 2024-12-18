﻿using ArchitectureTest.Databases.SqlServer.Entities;
using ArchitectureTest.Domain.Models.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArchitectureTest.Domain.Models;

public class ChecklistDTO : BasicDTO<long>, IEntityConverter<Checklist>, IChildEntityConverter<ChecklistDetail> {
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
    public IList<ChecklistDetailDTO>? Details { get; set; }
    public IList<ChecklistDetail>? GetChildEntities() {
        return Details?.Select(d => d.ToEntity())?.ToList();
    }

    public Checklist ToEntity() {
        return new Checklist {
            Id = Id,
            UserId = UserId,
            Title = Title,
            CreationDate = CreationDate,
            ModificationDate = ModificationDate
        };
    }
}

public class ChecklistDetailDTO : BasicDTO<long>, IEntityConverter<ChecklistDetail> {
    public long ChecklistId { get; set; }
    public long? ParentDetailId { get; set; }
    public string? TaskName { get; set; }
    public bool Status { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
    public IList<ChecklistDetailDTO>? SubItems { get; set; }

    public ChecklistDetail ToEntity() {
        return new ChecklistDetail {
            Id = Id,
            ChecklistId = ChecklistId,
            ParentDetailId = ParentDetailId,
            TaskName = TaskName,
            Status = Status,
            CreationDate = CreationDate,
            ModificationDate = ModificationDate
        };
    }
}
