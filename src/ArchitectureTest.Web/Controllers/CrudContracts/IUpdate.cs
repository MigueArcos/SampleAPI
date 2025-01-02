using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers.Contracts;

public interface IUpdate<T>
{
    Task<IActionResult> Update([FromRoute] string id, [FromBody] T input);
}
