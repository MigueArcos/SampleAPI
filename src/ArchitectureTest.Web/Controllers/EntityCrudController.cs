using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

public abstract class EntityCrudController<TEntity> : BaseController
    where TEntity : BaseEntity<long>
{
    protected readonly ICrudService<TEntity> _entityCrudService;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    public EntityCrudController(
        ICrudService<TEntity> entityCrudService, IHttpContextAccessor httpContextAccessor, ILogger<BaseController> logger
    ) : base(logger) 
    {
        _entityCrudService = entityCrudService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TEntity inputEntity)
    {
        var result = await _entityCrudService.Create(inputEntity).ConfigureAwait(false);

        if (result.Error is not null)
            return HandleError(result.Error);

        string location = $"{_httpContextAccessor.HttpContext?.Request.Path.Value}/{result.Value!.Id}";
        return Created(location, result.Value);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetById([FromRoute] long id)
    {
        var result = await _entityCrudService.GetById(id).ConfigureAwait(false);

        if (result.Error is not null)
            return HandleError(result.Error);

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public virtual async Task<IActionResult> Update([FromRoute] long id, [FromBody] TEntity inputEntity)
    {
        var result = await _entityCrudService.Update(id, inputEntity).ConfigureAwait(false);

        if (result.Error is not null)
            return HandleError(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> DeleteById([FromRoute] long id)
    {
        var result = await _entityCrudService.DeleteById(id).ConfigureAwait(false);

        if (result is not null)
            return HandleError(result);

        return NoContent();
    }
}
