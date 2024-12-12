using ArchitectureTest.Domain.Models.Enums;

namespace ArchitectureTest.Web.Configuration;

public class BadRequestHttpErrorInfo : HttpErrorInfo
{
    public BadRequestHttpErrorInfo() {
        ErrorCode = ErrorCodes.ValidationsFailed;
        HttpStatusCode = StatusCodes.Status400BadRequest;
    }

    public IList<HttpErrorInfo> Errors { get; set; }
}
