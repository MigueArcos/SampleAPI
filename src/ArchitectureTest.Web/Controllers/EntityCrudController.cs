using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using ArchitectureTest.Web.Controllers.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

public abstract class EntityCrudController<TEntity, TDto> : BaseController, IRead, IDelete, IUpdate<TDto>, ICreate<TDto>
    where TEntity : BaseEntity<string>
    where TDto : class
{
    protected readonly ICrudService<TEntity, TDto> _entityCrudService;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    public EntityCrudController(
        ICrudService<TEntity, TDto> entityCrudService, IHttpContextAccessor httpContextAccessor, ILogger<BaseController> logger
    ) : base(logger)
    {
        _entityCrudService = entityCrudService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TDto input)
    {
        var result = await _entityCrudService.Create(input).ConfigureAwait(false);

        if (result.Error is not null)
            return HandleError(result.Error);

        string location = $"{_httpContextAccessor.HttpContext?.Request.Path.Value}/{result.Value!.Id}";
        return Created(location, result.Value.Entity);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetById([FromRoute] string id)
    {
        var result = await _entityCrudService.GetById(id).ConfigureAwait(false);

        if (result.Error is not null)
            return HandleError(result.Error);

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public virtual async Task<IActionResult> Update([FromRoute] string id, [FromBody] TDto input)
    {
        var result = await _entityCrudService.Update(id, input).ConfigureAwait(false);

        if (result.Error is not null)
            return HandleError(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> DeleteById([FromRoute] string id)
    {
        var result = await _entityCrudService.DeleteById(id).ConfigureAwait(false);

        if (result is not null)
            return HandleError(result);

        return NoContent();
    }

    public abstract Task<IActionResult> GetAll();
}
