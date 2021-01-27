using ArchitectureTest.Domain.Models.Converters;
using ArchitectureTest.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService;
using System;

namespace ArchitectureTest.Web.Controllers {
    public abstract class EntityCrudController<TEntity, TDto> : BaseController where TEntity : class where TDto : BasicDTO, IEntityConverter<TEntity> {
		protected readonly BaseEntityCrud<TEntity, TDto> domain;
		private readonly bool validateEntityBelongsToUser;
		public EntityCrudController(BaseEntityCrud<TEntity, TDto> domain, bool validateEntityBelongsToUser = true) {
			this.domain = domain;
			this.validateEntityBelongsToUser = validateEntityBelongsToUser;
		}
		[HttpPost]
		public virtual async Task<IActionResult> Post([FromBody] TDto dto) {
			try {
				var result = await domain.Post(dto);
				return Ok(dto);
			}
			catch (Exception exception) {
				return DefaultCatch(exception);
			}
		}
		[HttpGet("{id}")]
		public virtual async Task<IActionResult> GetById([FromRoute] long id) {
			try {
				var result = await (validateEntityBelongsToUser ?
					domain.GetById(id, long.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value))
					:
					domain.GetById(id)
				);
				return Ok(result);
			}
			catch (Exception exception) {
				return DefaultCatch(exception);
			}
		}
		[HttpPut("{id}")]
		public virtual async Task<IActionResult> Put([FromRoute] long id, [FromBody] TDto dto) {
			try {
				var result = await (validateEntityBelongsToUser ?
					domain.Put(id, dto, long.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value))
					:
					domain.Put(id, dto)
				);
				return Ok(dto);
			}
			catch (Exception exception) {
				return DefaultCatch(exception);
			}
		}
		[HttpDelete("{id}")]
		public virtual async Task<IActionResult> Delete([FromRoute] long id) {
			try {
				var result = await (validateEntityBelongsToUser ?
					domain.Delete(id, long.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value))
					:
					domain.Delete(id)
				);
				return Ok(new { Id = id });
			}
			catch (Exception exception) {
				return DefaultCatch(exception);
			}
		}
	}
}
