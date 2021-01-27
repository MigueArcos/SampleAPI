using ArchitectureTest.Domain.Models.StatusCodes;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ArchitectureTest.Web.Controllers {
	public class BaseController : ControllerBase {
		protected ObjectResult DefaultCatch(Exception exception) {
			ErrorStatusCode error = null;
			if (exception is ErrorStatusCode) {
				error = exception as ErrorStatusCode;
			}
			else {
				//We should never expose real exceptions, so we will catch all unknown exceptions (DatabaseErrors, Null Errors, Index errors, etc...) and rethrow an UnknownError after log
				Console.WriteLine(exception);
				error = ErrorStatusCode.UnknownError;
			}
			return new ObjectResult(error.Detail) {
				StatusCode = error.HttpStatusCode
			};
		}
	}
}
