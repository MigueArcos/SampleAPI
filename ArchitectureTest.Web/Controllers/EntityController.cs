using ArchitectureTest.Domain.Contracts;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.StatusCodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.Controllers {
	public class EntityController<TEntity, TDto> : BaseController where TEntity : class where TDto : BasicDTO, IEntityConverter<TEntity> {
		protected readonly BaseDomain<TEntity, TDto> domain;
		private readonly bool validateEntityBelongsToUser;
		public EntityController(BaseDomain<TEntity, TDto> domain, bool validateEntityBelongsToUser = true) {
			this.domain = domain;
			this.validateEntityBelongsToUser = validateEntityBelongsToUser;
		}
		[HttpPost]
		[Authorize]
		public virtual async Task<IActionResult> Post([FromBody] TDto dto) {
			try {
				var result = await domain.Post(dto);
				return Ok(dto);
			}
			catch (ErrorStatusCode exception) {
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
			catch (ErrorStatusCode exception) {
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
			catch (ErrorStatusCode exception) {
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
			catch (ErrorStatusCode exception) {
				return DefaultCatch(exception);
			}
		}
		protected ObjectResult DefaultCatch(ErrorStatusCode error) {
			//var error = exception is ErrorStatusCode ? exception as ErrorStatusCode : ErrorStatusCode.UnknownError;
			return new ObjectResult(error.Detail) {
				StatusCode = error.HttpStatusCode
			};
		}
	}
}
