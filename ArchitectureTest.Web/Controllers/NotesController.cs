using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Domain.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	public class NotesController : BaseController<Note, NoteDTO> {
		private readonly IHttpContextAccessor httpContextAccessor;
		public NotesController(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : base(new NotesDomain(unitOfWork)) {
			this.httpContextAccessor = httpContextAccessor;
		}

		[HttpGet("list")]
		[Authorize]
		public async Task<ObjectResult> GetAll() {
			try {
				var userId = long.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);//Should be retrieved from token
				var result = await (domain as NotesDomain).GetUserNotes(userId);
				return Ok(result);
			}
			catch (ErrorStatusCode exception) {
				return DefaultCatch(exception);
			}
		}
	}
}
