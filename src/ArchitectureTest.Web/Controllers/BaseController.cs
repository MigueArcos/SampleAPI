using ArchitectureTest.Domain.Models;
using ArchitectureTest.Web.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureTest.Web.Controllers;

public class BaseController : ControllerBase {
    private readonly ILogger<BaseController> _logger;

    public BaseController(ILogger<BaseController> logger)
    {
        _logger = logger;
    }

    protected ObjectResult HandleError(AppError error) {
        var errorInfo = HttpResponses.TryGetErrorInfo(error.Code, message => _logger.LogError(message));
        return new ObjectResult(errorInfo) {
            StatusCode = errorInfo!.HttpStatusCode
        };
    }
}
