using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers.Contracts;

public interface ICreate<in T>
{
    Task<IActionResult> Create([FromBody] T input);
}
