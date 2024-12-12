namespace ArchitectureTest.Web.Configuration;

public class BadRequestHttpErrorInfo : HttpErrorInfo
{
    public BadRequestHttpErrorInfo() {
        HttpStatusCode = StatusCodes.Status400BadRequest;
    }

    public IList<HttpErrorInfo>? Errors { get; set; }
}
