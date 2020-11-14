using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.UnitOfWork;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureTest.Domain.StatusCodes;
using System.Security.Claims;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	public class ChecklistController : BaseController<Checklist, ChecklistDTO> {
		public ChecklistController(IUnitOfWork unitOfWork) : base(new ChecklistDomain(unitOfWork)) {

		}

		[HttpGet("list")]
		[Authorize]
		public async Task<ObjectResult> GetAll() {
			try {
				var userId = long.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);//Should be retrieved from token
				var result = await (domain as ChecklistDomain).GetUserChecklists(userId);
				return Ok(result);
			}
			catch (ErrorStatusCode exception) {
				return DefaultCatch(exception);
			}
		}
	}
}
