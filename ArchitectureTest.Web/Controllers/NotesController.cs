using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService;

namespace ArchitectureTest.Web.Controllers {
    [Route("api/[controller]")]
    [Authorize]
    public class NotesController : EntityCrudController<Note, NoteDTO> {
        public NotesController(BaseEntityCrud<Note, NoteDTO> domain) : base(domain) {
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAll() {
            try {
                var userId = long.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
                var result = await (domain as NotesCrudService).GetUserNotes(userId);
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
