using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	public class ChecklistController : EntityController<Checklist, ChecklistDTO> {
		public ChecklistController(IUnitOfWork unitOfWork) : base(new ChecklistDomain(unitOfWork)) {

		}

		[HttpGet("list")]
		[Authorize]
		public async Task<IActionResult> GetAll() {
			try {
				var userId = long.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);//Should be retrieved from token
				var result = await (domain as ChecklistDomain).GetUserChecklists(userId);
				return Ok(result);
			}
			catch (Exception error) {
				return DefaultCatch(error);
			}
		}
	}
}
