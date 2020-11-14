using System;

namespace ArchitectureTest.Domain.StatusCodes {
	public class ErrorStatusCode : Exception {
		public CustomCode StatusCode { get; set; }
		public int HttpStatusCode { get; set; }
		public ErrorStatusCode(int statusCode, int httpStatusCode, string error) {
			StatusCode = new CustomCode {
				StatusCode = statusCode,
				Message = error
			};
			HttpStatusCode = httpStatusCode;
		}
		public static readonly ErrorStatusCode UnknownError = new ErrorStatusCode(5000, 500, "Error desconocido");
		public static readonly ErrorStatusCode EmailAlreadyInUse = new ErrorStatusCode(4020, 400, "Este correo electrónico ya esta en uso");
		public static readonly ErrorStatusCode UserNameNotFound = new ErrorStatusCode(4102, 400, "Se debe especificar el nombre de usuario");
		public static readonly ErrorStatusCode WeakPassWord = new ErrorStatusCode(4021, 400, "Esta contraseña es demasiado debil, debe tener al menos 6 caracteres");
		public static readonly ErrorStatusCode PasswordsDoNotMatch = new ErrorStatusCode(4022, 400, "Las contraseñas no coinciden");

		public static readonly ErrorStatusCode UserNotFound = new ErrorStatusCode(4023, 404, "No se encontraron datos del usuario");
		public static readonly ErrorStatusCode InvalidPassword = new ErrorStatusCode(4024, 403, "La contraseña no es correcta");


		public static readonly ErrorStatusCode EmailInvalid = new ErrorStatusCode(4026, 400, "Email inválido");
		public static readonly ErrorStatusCode PasswordInvalid = new ErrorStatusCode(4027, 400, "Contraseña Invalida");

		public static readonly ErrorStatusCode AuthorizarionMissing = new ErrorStatusCode(4031, 401, "Es necesario estar autenticado");
		public static readonly ErrorStatusCode AuthorizationFailed = new ErrorStatusCode(4032, 401, "Falló la autenticación");
		public static readonly ErrorStatusCode NoteDoesNotExists = new ErrorStatusCode(4034, 404, "No se encontro esta nota");
		public static readonly ErrorStatusCode ChecklistDoesNotExists = new ErrorStatusCode(4035, 404, "No se encontro esta lista de tareas");

		public static readonly ErrorStatusCode NoteIdNotSupplied = new ErrorStatusCode(4036, 400, "Es necesario el identificador de nota");
		public static readonly ErrorStatusCode NoteTitleNotFound = new ErrorStatusCode(4037, 400, "Es necesario especificar un título de nota");
		public static readonly ErrorStatusCode UserIdNotSupplied = new ErrorStatusCode(4100, 400, "No se especifico identificador de usuario o es inválido");

		public static readonly ErrorStatusCode NoEmail = new ErrorStatusCode(4101, 400, "Es necesario especificar un correo electrónico");
		public static readonly ErrorStatusCode RepoProblem = new ErrorStatusCode(5001, 500, "Hay algún problema con los repositorios de datos");
	}
}
