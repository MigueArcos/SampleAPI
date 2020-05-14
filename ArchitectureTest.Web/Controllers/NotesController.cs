using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Data.StatusCodes;
using ArchitectureTest.Data.UnitOfWork;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	public class NotesController : BaseController<Note, NoteDTO> {
		public NotesController(IUnitOfWork unitOfWork) : base(new NotesDomain(unitOfWork.NotesRepository, unitOfWork)) {

		}
		[HttpGet]
		public async Task<ObjectResult> GetAll() {
			try {
				long userId = 1;//Should be retrieved from token
				var result = await (domain as NotesDomain).GetUserNotes(userId);
				return Ok(result);
			}
			catch (ErrorStatusCode exception) {
				return DefaultCatch(exception);
			}
		}
	}
}
