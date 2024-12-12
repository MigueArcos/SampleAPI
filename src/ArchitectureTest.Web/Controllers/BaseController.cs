using ArchitectureTest.Domain.Models.Enums;
using ArchitectureTest.Web.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

public class BaseController : ControllerBase {
	private readonly ILogger<BaseController> _logger;

    public BaseController(ILogger<BaseController> logger)
    {
        _logger = logger;
    }

	protected ObjectResult DefaultCatch(Exception exception) {
		var errorInfo = GetHttpErrorFromException(exception, _logger);
        return new ObjectResult(errorInfo) {
            StatusCode = errorInfo!.HttpStatusCode
        };
	}

	public static HttpErrorInfo GetHttpErrorFromException(Exception exception, ILogger logger) {
		var isAManagedError = HttpResponses.CommonErrors.TryGetValue(exception.Message, out var errorInfo);
        if (!isAManagedError) {
            // We should never expose real exceptions, so we will catch all unknown exceptions 
            // (DatabaseErrors, Null Errors, Index errors, etc...) and rethrow an UnknownError after log
            logger.LogInformation(exception.Message);
            errorInfo = HttpResponses.CommonErrors[ErrorCodes.UnknownError];
        }
		return errorInfo;
	}
}
