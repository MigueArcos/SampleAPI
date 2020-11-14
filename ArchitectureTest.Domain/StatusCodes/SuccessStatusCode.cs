namespace ArchitectureTest.Domain.StatusCodes {
	public class SuccessStatusCode : CustomCode {
		public int HttpStatusCode { get; set; } = 200;
		public SuccessStatusCode(int statusCode, int httpStatusCode, string message) {
			StatusCode = statusCode;
			HttpStatusCode = httpStatusCode;
			Message = message;
		}

		public SuccessStatusCode(int statusCode, string message) {
			StatusCode = statusCode;
			Message = message;
		}
		public static readonly SuccessStatusCode RepoNotFound = new SuccessStatusCode(2000, "Todo bien");
	}
}
