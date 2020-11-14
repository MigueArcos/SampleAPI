using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArchitectureTest.Domain.Models {
	public class ChecklistDTO : BasicDTO, IEntityConverter<Checklist>, IChildEntityConverter<ChecklistDetail> {
		public long UserId { get; set; }
		public string Title { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime ModificationDate { get; set; }
		public List<ChecklistDetailDTO> Details { get; set; }
		public List<ChecklistDetail> GetChildEntities() {
			return Details.Select(d => d.ToEntity()).ToList();
		}

		public Checklist ToEntity() {
			return new Checklist {
				Id = Id ?? 0,
				UserId = UserId,
				Title = Title,
				CreationDate = CreationDate,
				ModificationDate = ModificationDate
			};
		}
	}
	public class ChecklistDetailDTO : BasicDTO, IEntityConverter<ChecklistDetail> {
		public long ChecklistId { get; set; }
		public long? ParentDetailId { get; set; }
		public string TaskName { get; set; }
		public bool Status { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime ModificationDate { get; set; }

		public ChecklistDetail ToEntity() {
			return new ChecklistDetail {
				Id = Id ?? 0,
				ChecklistId = ChecklistId,
				ParentDetailId = ParentDetailId,
				TaskName = TaskName,
				Status = Status,
				CreationDate = CreationDate,
				ModificationDate = ModificationDate
			};
		}
	}
}
