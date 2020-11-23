using System;
using System.Collections.Generic;

namespace ArchitectureTest.Data.Database.SQLServer.Entities
{
    public partial class TokenType
    {
        public TokenType()
        {
            UserToken = new HashSet<UserToken>();
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public ICollection<UserToken> UserToken { get; set; }
    }
}
