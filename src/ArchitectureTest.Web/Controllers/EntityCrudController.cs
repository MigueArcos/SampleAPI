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
		try {
			TDto result = await _entityCrudService.Add(dto);
			string location = $"{_httpContextAccessor.HttpContext?.Request.Path.Value}/{result.Id}";
			return Created(location, result);
		}
		catch (Exception exception) {
			return DefaultCatch(exception);
		}
	}

	[HttpGet("{id}")]
	public virtual async Task<IActionResult> GetById([FromRoute] long id) {
		try {
			var result = await _entityCrudService.GetById(id);
			return Ok(result);
		}
		catch (Exception exception) {
			return DefaultCatch(exception);
		}
	}

	[HttpPut("{id}")]
	public virtual async Task<IActionResult> Put([FromRoute] long id, [FromBody] TDto dto) {
		try {
			var result = await _entityCrudService.Update(id, dto);
			return Ok(result);
		}
		catch (Exception exception) {
			return DefaultCatch(exception);
		}
	}

	[HttpDelete("{id}")]
	public virtual async Task<IActionResult> Delete([FromRoute] long id) {
		try {
			var result = await _entityCrudService.Delete(id);
			return NoContent();
		}
		catch (Exception exception) {
			return DefaultCatch(exception);
		}
	}
}
