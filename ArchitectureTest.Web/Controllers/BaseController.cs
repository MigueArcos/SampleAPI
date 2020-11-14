using ArchitectureTest.Domain.Contracts;
using ArchitectureTest.Domain.Domain;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.StatusCodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArchitectureTest.Web.Controllers {
	public class BaseController<TEntity, TDto> : ControllerBase where TEntity : class where TDto : BasicDTO, IEntityConverter<TEntity> {
		protected readonly BaseDomain<TEntity, TDto> domain;
		public BaseController(BaseDomain<TEntity, TDto> domain) {
			this.domain = domain;
		}
		[HttpPost]
		[Authorize]
		public async Task<ObjectResult> Post([FromBody] TDto dto) {
			try {
				var result = await domain.Post(dto);
				return Ok(dto);
			}
			catch (ErrorStatusCode exception) {
				return DefaultCatch(exception);
			}
		}
		[HttpGet("{id}")]
		public async Task<ObjectResult> GetById([FromRoute] long id) {
			try {
				var result = await domain.GetById(id);
				return Ok(result);
			}
			catch (ErrorStatusCode exception) {
				return DefaultCatch(exception);
			}
		}
		[HttpPut("{id}")]
		public async Task<ObjectResult> Put([FromRoute] long id, [FromBody] TDto dto) {
			try {
				var result = await domain.Put(id, dto);
				return Ok(dto);
			}
			catch (ErrorStatusCode exception) {
				return DefaultCatch(exception);
			}
		}
		[HttpDelete("{id}")]
		public async Task<ObjectResult> Delete([FromRoute] long id) {
			try {
				var result = await domain.Delete(id);
				return Ok(new { Id = id });
			}
			catch (ErrorStatusCode exception) {
				return DefaultCatch(exception);
			}
		}
		protected ObjectResult DefaultCatch(ErrorStatusCode error) {
			//var error = exception is ErrorStatusCode ? exception as ErrorStatusCode : ErrorStatusCode.UnknownError;
			return new ObjectResult(error.StatusCode) {
				StatusCode = error.HttpStatusCode
			};
		}
	}
}
