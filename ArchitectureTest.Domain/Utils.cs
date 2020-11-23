using ArchitectureTest.Domain.StatusCodes;
using System;

namespace ArchitectureTest.Domain {
	public class Utils {
		public static ErrorStatusCode HandleException(Exception exception) {
			ErrorStatusCode error = null;
			if (exception is ErrorStatusCode) {
				error = exception as ErrorStatusCode;
			}
			else {
				//We should never expose real exceptions, so we will catch all unknown exceptions (DatabaseErrors, Null Errors, Index errors, etc...) and rethrow an UnknownError after log
				Console.WriteLine(exception);
				error = ErrorStatusCode.UnknownError;
			}
			return error;
		}
	}
}
