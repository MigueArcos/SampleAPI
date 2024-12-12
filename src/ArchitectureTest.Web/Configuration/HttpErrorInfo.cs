using System.Text.Json.Serialization;

namespace ArchitectureTest.Web.Configuration;

public class HttpErrorInfo
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int HttpStatusCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ErrorCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Message { get; set; }
}

