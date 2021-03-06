﻿using System;
using System.Collections.Generic;

namespace ArchitectureTest.Data.Database.MySQL.Entities
{
    public partial class User {
        public User()
        {
            Checklist = new HashSet<Checklist>();
            Note = new HashSet<Note>();
        }
        public long Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ModificationDate { get; set; }

        public ICollection<Checklist> Checklist { get; set; }
        public ICollection<Note> Note { get; set; }
    }
}
