using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Data.UnitOfWork;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	public class ChecklistController : BaseController<Checklist, ChecklistDTO> {
		public ChecklistController(IUnitOfWork unitOfWork) : base(new ChecklistDomain(unitOfWork.ChecklistRepository, unitOfWork)) {

		}
	}
}
