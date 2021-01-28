using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Converters;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.Controllers {
	public abstract class EntityCrudController<TEntity, TDto> : BaseController where TEntity : class where TDto : BasicDTO, IEntityConverter<TEntity> {
		protected readonly EntityCrudService<TEntity, TDto> entityCrudService;
		public EntityCrudController(EntityCrudService<TEntity, TDto> entityCrudService) {
			this.entityCrudService = entityCrudService;
		}
		[HttpPost]
		public virtual async Task<IActionResult> Post([FromBody] TDto dto) {
			try {
				var result = await entityCrudService.Post(dto);
				return Ok(dto);
			}
			catch (Exception exception) {
				return DefaultCatch(exception);
			}
		}
		[HttpGet("{id}")]
		public virtual async Task<IActionResult> GetById([FromRoute] long id) {
			try {
				var result = await entityCrudService.GetById(id);
				return Ok(result);
			}
			catch (Exception exception) {
				return DefaultCatch(exception);
			}
		}
		[HttpPut("{id}")]
		public virtual async Task<IActionResult> Put([FromRoute] long id, [FromBody] TDto dto) {
			try {
				var result = await entityCrudService.Put(id, dto);
				return Ok(dto);
			}
			catch (Exception exception) {
				return DefaultCatch(exception);
			}
		}
		[HttpDelete("{id}")]
		public virtual async Task<IActionResult> Delete([FromRoute] long id) {
			try {
				var result = await entityCrudService.Delete(id);
				return Ok(new { Id = id });
			}
			catch (Exception exception) {
				return DefaultCatch(exception);
			}
		}
	}
}
