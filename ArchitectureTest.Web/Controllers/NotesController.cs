﻿using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Domain.UnitOfWork;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ArchitectureTest.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using ArchitectureTest.Web.ActionFilters;

namespace ArchitectureTest.Web.Controllers {
	[Route("api/[controller]")]
	public class NotesController : BaseController<Note, NoteDTO> {
		private readonly IHttpContextAccessor httpContextAccessor;
		public NotesController(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : base(new NotesDomain(unitOfWork.NotesRepository, unitOfWork)) {
			this.httpContextAccessor = httpContextAccessor;
		}

		[HttpGet]
		[TypeFilter(typeof(ValidateJwt))]
		public async Task<ObjectResult> GetAll() {
			try {
				var userId = long.Parse(httpContextAccessor.HttpContext.Items[AppConstants.UserId].ToString());//Should be retrieved from token
				var result = await (domain as NotesDomain).GetUserNotes(userId);
				return Ok(result);
			}
			catch (ErrorStatusCode exception) {
				return DefaultCatch(exception);
			}
		}
	}
}
