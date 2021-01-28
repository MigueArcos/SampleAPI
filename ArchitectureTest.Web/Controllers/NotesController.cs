using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService;
using ArchitectureTest.Web.Services.UserIdentity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	[Authorize]
	public class NotesController : EntityCrudController<Note, NoteDTO> {
		public NotesController(EntityCrudService<Note, NoteDTO> entityCrudService, IClaimsUserAccesor<JwtUser> claimsUserAccesor) : base(entityCrudService) {
			long userId = claimsUserAccesor.GetUser()?.Id ?? 0;
			entityCrudService.CrudSettings = new EntityCrudSettings {
				ValidateEntityBelongsToUser = true,
				UserId = userId
			};
		}

		[HttpGet("list")]
		public async Task<IActionResult> GetAll() {
			try {
				var result = await (entityCrudService as NotesCrudService).GetUserNotes();
				return Ok(result);
			}
			catch (Exception error) {
				return DefaultCatch(error);
			}
		}
		/// We can add Authorize attribute for each method (overriding the default one and calling super.Method()), but in this case we are using the attribute at class Level, check out this answer for more details
		/// https://stackoverflow.com/questions/48198071/add-attribute-to-inherited-function-c-sharp#answer-48198206
	}
}
