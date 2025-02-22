﻿using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using ArchitectureTest.Web.Controllers.Contracts;
using ArchitectureTest.Web.HttpExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ChecklistController : EntityCrudController<Checklist, ChecklistDTO>, IUpdate<UpdateChecklistDTO> {
    public ChecklistController(
        IChecklistCrudService entityCrudService, 
        IHttpContextAccessor httpContextAccesor, 
        ILogger<ChecklistController> logger
    ) : base(entityCrudService, httpContextAccesor, logger)
    {
        string? userId = httpContextAccesor.GetUserIdentity()?.UserId;
        entityCrudService.CrudSettings = new EntityCrudSettings {
            ValidateEntityBelongsToUser = true,
            UserId = userId
        };
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll()
    {
        var result = await (_entityCrudService as IChecklistCrudService)!.GetUserChecklists().ConfigureAwait(false);

        if (result.Error is not null)
            return HandleError(result.Error);

        return Ok(result.Value);
    }

    // Overrides original method, the HttpPut attribute must be modified to prevent routing errors
    [HttpPut]
    public override Task<IActionResult> Update([FromRoute] string id, [FromBody] ChecklistDTO input) 
        => Update(id, (UpdateChecklistDTO) input);


    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateChecklistDTO input)
    {
        var result = await base.Update(id, input).ConfigureAwait(false);
        return result;
    }

    // [HttpPut("{id}")]
    // public override async Task<IActionResult> Update([FromRoute] string id, [FromBody] Checklist input)
    // {
    //     var reader = new StreamReader(Request.Body);
    //     reader.BaseStream.Seek(0, SeekOrigin.Begin); 
    //     var rawMessage = reader.ReadToEnd(); // This does not work
    //     var k = input as TestTac;
    //     var s = await base.Update(id, input);
    //     return s;
    // }

    // Note
    // We can add Authorize attribute for each method (overriding the default one and calling super.Method()), 
    // but in this case we are using the attribute at class Level, check out this answer for more details
    // https://stackoverflow.com/questions/48198071/add-attribute-to-inherited-function-c-sharp#answer-48198206
}
