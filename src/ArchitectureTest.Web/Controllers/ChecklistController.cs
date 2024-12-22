﻿using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.NewImpl;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.NewImpl.Contracts;
using ArchitectureTest.Web.HttpExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ChecklistController : EntityCrudController<ChecklistEntity> {
    public ChecklistController(
        IChecklistCrudService entityCrudService, 
        IHttpContextAccessor httpContextAccesor, 
        ILogger<ChecklistController> logger
    ) : base(entityCrudService, httpContextAccesor, logger)
    {
        long userId = httpContextAccesor.GetUserIdentity().UserId;
        entityCrudService.CrudSettings = new EntityCrudSettings {
            ValidateEntityBelongsToUser = true,
            UserId = userId
        };
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAll() {
        var result = await (_entityCrudService as IChecklistCrudService)!.GetUserChecklists().ConfigureAwait(false);

        if (result.Error is not null)
            return HandleError(result.Error);

        return Ok(result.Value);
    }
    /// We can add Authorize attribute for each method (overriding the default one and calling super.Method()), 
    /// but in this case we are using the attribute at class Level, check out this answer for more details
    /// https://stackoverflow.com/questions/48198071/add-attribute-to-inherited-function-c-sharp#answer-48198206
}
