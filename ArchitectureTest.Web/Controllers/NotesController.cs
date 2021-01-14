using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Domain.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	public class NotesController : EntityController<Note, NoteDTO> {
		public NotesController(BaseDomain<Note, NoteDTO> domain) : base(domain) {
		}

		[HttpGet("list")]
		[Authorize]
		public async Task<IActionResult> GetAll() {
			try {
				var userId = long.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
				var result = await (domain as NotesDomain).GetUserNotes(userId);
				return Ok(result);
			}
			catch (Exception error) {
				return DefaultCatch(error);
			}
		}
	}
}
