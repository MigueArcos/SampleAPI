using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Converters;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

public abstract class EntityCrudController<TEntity, TDto> : BaseController
	where TEntity : class
	where TDto : BasicDTO<long>, IEntityConverter<TEntity>
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
	public virtual async Task<IActionResult> Post([FromBody] TDto dto) {
		var result = await _entityCrudService.Add(dto).ConfigureAwait(false);

		if (result.Error is not null)
			return HandleError(result.Error);

		string location = $"{_httpContextAccessor.HttpContext?.Request.Path.Value}/{result.Value!.Id}";
		return Created(location, result.Value);
	}

	[HttpGet("{id}")]
	public virtual async Task<IActionResult> GetById([FromRoute] long id) {
		var result = await _entityCrudService.GetById(id).ConfigureAwait(false);

		if (result.Error is not null)
			return HandleError(result.Error);

		return Ok(result.Value);
	}

	[HttpPut("{id}")]
	public virtual async Task<IActionResult> Put([FromRoute] long id, [FromBody] TDto dto) {
		var result = await _entityCrudService.Update(id, dto).ConfigureAwait(false);

		if (result.Error is not null)
			return HandleError(result.Error);

		return Ok(result.Value);
	}

	[HttpDelete("{id}")]
	public virtual async Task<IActionResult> Delete([FromRoute] long id) {
		var result = await _entityCrudService.Delete(id).ConfigureAwait(false);

		if (result is not null)
			return HandleError(result);

		return NoContent();
	}
}
