using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService.Contracts;
using ArchitectureTest.Domain.ServicesLayer.EntityCrudService.Contracts;
using ArchitectureTest.Web.HttpExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ChecklistController : EntityCrudController<Checklist, ChecklistDTO> {
	public ChecklistController(
		ICrudService<Checklist, ChecklistDTO> entityCrudService, 
		IHttpContextAccessor httpContextAccesor, 
		ILogger<ChecklistController> logger
	) : base(entityCrudService, httpContextAccesor, logger)
	{
		long userId = httpContextAccesor.GetUserIdentity().UserId;
		entityCrudService.CrudSettings = new EntityCrudSettings {
			ValidateEntityBelongsToUser = true,
			UserId = userId
		};
	}

	[HttpGet("list")]
	public async Task<IActionResult> GetAll() {
		try {
			var result = await (entityCrudService as IChecklistCrudService).GetUserChecklists();
			return Ok(result);
		}
		catch (Exception error) {
			return DefaultCatch(error);
		}
	}
	/// We can add Authorize attribute for each method (overriding the default one and calling super.Method()), 
	/// but in this case we are using the attribute at class Level, check out this answer for more details
	/// https://stackoverflow.com/questions/48198071/add-attribute-to-inherited-function-c-sharp#answer-48198206
}
