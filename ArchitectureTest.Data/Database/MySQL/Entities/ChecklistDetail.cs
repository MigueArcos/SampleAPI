using System;
using System.Collections.Generic;

namespace ArchitectureTest.Data.Database.MySQL.Entities
{
    public partial class ChecklistDetail : Entity {
        public long ChecklistId { get; set; }
        public long? ParentDetailId { get; set; }
        public string TaskName { get; set; }
        public bool Status { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ModificationDate { get; set; }

        public Checklist Checklist { get; set; }
    }
}
