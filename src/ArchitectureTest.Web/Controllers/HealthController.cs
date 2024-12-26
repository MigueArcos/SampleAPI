using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public HealthInfo GetHealth()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        return new HealthInfo {
            Status = "Up",
            Version = informationVersion ?? "local"
        };
    }

    public sealed class HealthInfo
    {
        public required string Status { get; set; }
        public required string Version { get; set; }
    }
}
