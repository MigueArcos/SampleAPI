using System;
using System.Collections.Generic;

namespace ArchitectureTest.Data.Database.SQLServer.Entities
{
    public partial class Checklist
    {
        public Checklist()
        {
            ChecklistDetail = new HashSet<ChecklistDetail>();
        }

        public long Id { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ModificationDate { get; set; }

        public User User { get; set; }
        public ICollection<ChecklistDetail> ChecklistDetail { get; set; }
    }
}
