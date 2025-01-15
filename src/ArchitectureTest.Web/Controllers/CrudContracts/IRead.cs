using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers.Contracts;

public interface IRead
{
    Task<IActionResult> GetById([FromRoute] string id);

    Task<IActionResult> GetAll();
}
