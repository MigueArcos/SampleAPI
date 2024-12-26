using System.Text;
using System.Text.Json;
using ArchitectureTest.Web.Configuration;
using ArchitectureTest.Web.HttpExtensions;
using Microsoft.AspNetCore.Diagnostics;

namespace ArchitectureTest.Web;

public class GlobalHttpExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalHttpExceptionHandler> _logger;

    public GlobalHttpExceptionHandler(ILogger<GlobalHttpExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken
    ){   
        var (userId, email, name) = httpContext.GetUserIdentity();
        if (userId == 0)
            _logger.LogError(exception, "An exception occurred");
        else
            _logger.LogError(exception, "An exception occurred: UserId = {UserId}, Email = {Email}", userId, email);

        var errorInfo = HttpResponses.TryGetErrorInfo(exception.Message, _ => {});

        var json = JsonSerializer.Serialize(errorInfo);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        httpContext.Response.StatusCode = errorInfo!.HttpStatusCode;
        httpContext.Response.Headers.Append("Content-Type", "application/json");
        await httpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        return true;
    }
}
