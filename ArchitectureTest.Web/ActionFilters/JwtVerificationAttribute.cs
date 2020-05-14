using ArchitectureTest.Domain.StatusCodes;
using ArchitectureTest.Infrastructure.Extensions;
using ArchitectureTest.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ArchitectureTest.Web.ActionFilters {
	public class JwtVerificationAttribute : IActionFilter {
		private readonly JwtTokenManager jwtTokenManager;
		private readonly IHttpContextAccessor httpContextAccessor;
		public JwtVerificationAttribute(JwtTokenManager jwtTokenManager, IHttpContextAccessor httpContextAccessor) {
			this.jwtTokenManager = jwtTokenManager;
			this.httpContextAccessor = httpContextAccessor;
		}

		public void OnActionExecuted(ActionExecutedContext context) { }

		public void OnActionExecuting(ActionExecutingContext context) {
			var jwtToken = httpContextAccessor.HttpContext.GetCookieValue(AppConstants.JwtCookieName);
			if (!string.IsNullOrEmpty(jwtToken)) {
				if (jwtTokenManager.TokenIsValid(jwtToken)){
					httpContextAccessor.HttpContext.Items[AppConstants.UserId] = jwtTokenManager.GetClaim(jwtToken, AppConstants.UserId);
				}
				else{
					var error = ErrorStatusCode.AuthorizationFailed;
					context.Result = new ObjectResult(error.StatusCode) {
						StatusCode = error.HttpStatusCode
					};
					return;
				}
			}
			else{
				var error = ErrorStatusCode.AuthorizarionMissing;
				context.Result = new ObjectResult(error.StatusCode) {
					StatusCode = error.HttpStatusCode
				};
				return;
			}
		}
	}
}

