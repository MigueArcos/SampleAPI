using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Domain.UnitOfWork;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ArchitectureTest.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using ArchitectureTest.Web.ActionFilters;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	public class NotesController : BaseController<Note, NoteDTO> {
		private readonly IHttpContextAccessor httpContextAccessor;
		public NotesController(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : base(new NotesDomain(unitOfWork.NotesRepository, unitOfWork)) {
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
