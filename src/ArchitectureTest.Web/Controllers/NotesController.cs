using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using ArchitectureTest.Web.HttpExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class NotesController : EntityCrudController<Note, NoteDTO> {
    public NotesController(
        ICrudService<Note, NoteDTO> entityCrudService, IHttpContextAccessor httpContextAccesor, ILogger<NotesController> logger
    ) : base(entityCrudService, httpContextAccesor, logger) {
        long userId = httpContextAccesor.GetUserIdentity().UserId;
        entityCrudService.CrudSettings = new EntityCrudSettings {
            ValidateEntityBelongsToUser = true,
            UserId = userId
        };
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAll() {
        var result = await (_entityCrudService as INotesCrudService)!.GetUserNotes().ConfigureAwait(false);

        if (result.Error is not null)
            return HandleError(result.Error);

        return Ok(result.Value);
    }
    /// We can add Authorize attribute for each method (overriding the default one and calling super.Method()), 
    /// but in this case we are using the attribute at class Level, check out this answer for more details
    /// https://stackoverflow.com/questions/48198071/add-attribute-to-inherited-function-c-sharp#answer-48198206
}
