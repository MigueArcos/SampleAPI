using System;

namespace ArchitectureTest.Domain.StatusCodes {
	public class CustomMessages {
		public const string UnknownError = "Error desconocido";
		public const string EmailAlreadyInUse = "Este correo electrónico ya esta en uso";
		public const string UserNameNotFound = "Se debe especificar el nombre de usuario";
		public const string WeakPassWord = "Esta contraseña es demasiado debil, debe tener al menos 6 caracteres";
		public const string PasswordsDoNotMatch = "Las contraseñas no coinciden";
		public const string UserNotFound = "No se encontraron datos del usuario";
		public const string InvalidPassword = "La contraseña no es válida";
		public const string InvalidEmail = "Correo electrónico inválido";
		public const string WrongPassword = "Contraseña incorrecta";
		public const string AuthorizarionMissing = "Es necesario estar autenticado";
		public const string AuthorizationFailed = "Falló la autenticación";
		public const string NoteDoesNotExists = "No se encontro esta nota";
		public const string ChecklistDoesNotExists = "No se encontro esta lista de tareas";
		public const string EntityNotFound = "No se encontro información";
		public const string NoteIdNotSupplied = "Es necesario el identificador de nota";
		public const string NoteTitleNotFound = "Es necesario especificar un título de nota";
		public const string UserIdNotSupplied = "No se especifico identificador de usuario o es inválido";
		public const string NoEmail = "Es necesario especificar un correo electrónico";
		public const string RepoProblem = "Hay algún problema con los repositorios de datos";
		public const string InvalidUserName = "El nombre de usuario debe tener al menos 4 caracteres";
		public const string RefreshTokenExpired = "El token de refresco no existe";
		public const string EntityDoesNotBelongToUser = "Estos datos no le pertenecen al usuario";
		public const string EverythingOK = "Todo bien";
	}

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
		public static readonly ErrorStatusCode UnknownError = new ErrorStatusCode(5000, 500, CustomMessages.UnknownError);
		public static readonly ErrorStatusCode EmailAlreadyInUse = new ErrorStatusCode(4020, 400, CustomMessages.EmailAlreadyInUse);
		public static readonly ErrorStatusCode UserNameNotFound = new ErrorStatusCode(4102, 400, CustomMessages.UserNameNotFound);
		public static readonly ErrorStatusCode WeakPassWord = new ErrorStatusCode(4021, 400, CustomMessages.WeakPassWord);
		public static readonly ErrorStatusCode PasswordsDoNotMatch = new ErrorStatusCode(4022, 400, CustomMessages.PasswordsDoNotMatch);

		public static readonly ErrorStatusCode UserNotFound = new ErrorStatusCode(4023, 404, CustomMessages.UserNotFound);
		public static readonly ErrorStatusCode InvalidPassword = new ErrorStatusCode(4024, 403, CustomMessages.InvalidPassword);


		public static readonly ErrorStatusCode InvalidEmail = new ErrorStatusCode(4026, 400, CustomMessages.InvalidEmail);
		public static readonly ErrorStatusCode WrongPassword = new ErrorStatusCode(4027, 400, CustomMessages.WrongPassword);

		public static readonly ErrorStatusCode AuthorizarionMissing = new ErrorStatusCode(4031, 401, CustomMessages.AuthorizarionMissing);
		public static readonly ErrorStatusCode AuthorizationFailed = new ErrorStatusCode(4032, 401, CustomMessages.AuthorizationFailed);
		public static readonly ErrorStatusCode NoteDoesNotExists = new ErrorStatusCode(4034, 404, CustomMessages.NoteDoesNotExists);
		public static readonly ErrorStatusCode ChecklistDoesNotExists = new ErrorStatusCode(4035, 404, CustomMessages.ChecklistDoesNotExists);
		public static readonly ErrorStatusCode EntityNotFound = new ErrorStatusCode(4050, 404, CustomMessages.EntityNotFound);

		public static readonly ErrorStatusCode NoteIdNotSupplied = new ErrorStatusCode(4036, 400, CustomMessages.NoteIdNotSupplied);
		public static readonly ErrorStatusCode NoteTitleNotFound = new ErrorStatusCode(4037, 400, CustomMessages.NoteTitleNotFound);
		public static readonly ErrorStatusCode UserIdNotSupplied = new ErrorStatusCode(4100, 400, CustomMessages.UserIdNotSupplied);

		public static readonly ErrorStatusCode NoEmail = new ErrorStatusCode(4101, 400, CustomMessages.NoEmail);
		public static readonly ErrorStatusCode RepoProblem = new ErrorStatusCode(5001, 500, CustomMessages.RepoProblem);
		public static readonly ErrorStatusCode RefreshTokenExpired = new ErrorStatusCode(4102, 500, CustomMessages.RefreshTokenExpired);
		public static readonly ErrorStatusCode EntityDoesNotBelongToUser = new ErrorStatusCode(4200, 403, CustomMessages.EntityDoesNotBelongToUser);
	}
}
