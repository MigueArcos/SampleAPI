using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers.Contracts;

public interface IDelete
{
    Task<IActionResult> GetById([FromRoute] string id);
}
