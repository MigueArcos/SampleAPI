using System;
using System.Collections.Generic;
using System.Text;

namespace ArchitectureTest.Infrastructure.Jwt.Models {
	public class JsonWebToken {
		public long UserId { get; set; }
		public string Email { get; set; }
		public string Token { get; set; }
		public string RefreshToken { get; set; }
		public long ExpiresIn { get; set; }
	}
}
