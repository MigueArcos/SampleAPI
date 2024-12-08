namespace ArchitectureTest.Infrastructure.AppConfiguration;

public class ConfigData {
	public string FirebaseApiKey { get; set; }
	public string AuthCookie { get; set; }

	public JwtConfiguration Jwt { get; set; }
}

public class JwtConfiguration {
	public string Issuer { get; set; }
	public string Audience { get; set; }
	public string Secret { get; set; }
}
