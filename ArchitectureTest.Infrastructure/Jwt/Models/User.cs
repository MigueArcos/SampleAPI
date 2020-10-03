using System;
using System.Collections.Generic;
using System.Text;

namespace ArchitectureTest.Infrastructure.Jwt.Models {
	public class JwtUser {
		public long Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
	}
}
