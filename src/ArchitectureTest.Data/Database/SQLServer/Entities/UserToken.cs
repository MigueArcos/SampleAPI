using System;
using System.Collections.Generic;

namespace ArchitectureTest.Data.Database.SQLServer.Entities
{
    public partial class UserToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long TokenTypeId { get; set; }
        public string Token { get; set; }
        public DateTime? ExpiryTime { get; set; }

        public TokenType TokenType { get; set; }
        public User User { get; set; }
    }
}
