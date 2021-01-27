namespace ArchitectureTest.Domain.Models.StatusCodes {
    public struct SuccessMessages {
        public const string EverythingOK = "Todo bien";
    }
    public struct SuccessCodes {
        public const string EverythingOK = "success";
    }
    public class SuccessDetail {
        public string Message { get; set; }
        public string Code { get; set; }
    }
    public class SuccessStatusCode {
		public int HttpStatusCode { get; set; } = 200;
        public SuccessDetail Detail { get; set; }
		public SuccessStatusCode(int httpStatusCode, string message) {
			HttpStatusCode = httpStatusCode;
			Detail = new SuccessDetail {
                Message = message
            };
		}
        public SuccessStatusCode(int httpStatusCode, string message, string code) {
            HttpStatusCode = httpStatusCode;
            Detail = new SuccessDetail {
                Message = message,
                Code = code
            };
        }
        public static readonly SuccessStatusCode EverythingOK = new SuccessStatusCode(200, SuccessMessages.EverythingOK, SuccessCodes.EverythingOK);
	}
}
