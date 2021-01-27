using System;

namespace ArchitectureTest.Domain.StatusCodes {
    
    public struct ErrorMessages {
		public const string UnknownError = "Error desconocido";

        public const string InvalidEmail = "Correo electrónico inválido";
        public const string InvalidPassword = "La contraseña no es válida";
        public const string UserNameNotFound = "Se debe especificar el nombre de usuario";
        public const string WeakPassWord = "Esta contraseña es demasiado debil, debe tener al menos 6 caracteres";
        public const string PasswordsDoNotMatch = "Las contraseñas no coinciden";
        public const string EmailAlreadyInUse = "Este correo electrónico ya esta en uso";
        public const string WrongPassword = "Contraseña incorrecta";
        public const string AuthorizarionMissing = "Es necesario estar autenticado";
        public const string AuthorizationFailed = "Falló la autenticación";
        public const string UserNotFound = "No se encontraron datos del usuario";
        public const string NoEmail = "Es necesario especificar un correo electrónico";
        public const string InvalidUserName = "El nombre de usuario debe tener al menos 4 caracteres";
        public const string RefreshTokenExpired = "El token de refresco no existe";

        public const string NoteIdNotSupplied = "Es necesario el identificador de nota";
        public const string NoteDoesNotExists = "No se encontro esta nota";
        public const string NoteTitleNotFound = "Es necesario especificar un título de nota";

        public const string ChecklistIdNotSupplied = "Es necesario el identificador de la lista de tareas";
        public const string ChecklistDoesNotExists = "No se encontro esta lista de tareas";

        public const string UserIdNotSupplied = "No se especifico identificador de usuario o es inválido";

        public const string EntityNotFound = "No se encontro información";
        public const string EntityDoesNotBelongToUser = "Estos datos no le pertenecen al usuario";

        public const string RepoProblem = "Hay algún problema con los repositorios de datos";
	}
    
    public struct ErrorCodes {
        public const string UnknownError = "unknown";

        public const string InvalidEmail = "auth-0001";
        public const string InvalidPassword = "auth-0002";
        public const string UserNameNotFound = "auth-0003";
        public const string WeakPassWord = "auth-0004";
        public const string PasswordsDoNotMatch = "auth-0005";
        public const string EmailAlreadyInUse = "auth-0006";
        public const string WrongPassword = "auth-0007";
        public const string AuthorizarionMissing = "auth-0008";
        public const string AuthorizationFailed = "auth-0009";
        public const string UserNotFound = "auth-0010";
        public const string NoEmail = "auth-0011";
        public const string InvalidUserName = "auth-0012";
        public const string RefreshTokenExpired = "auth-0013";

        public const string NoteIdNotSupplied = "notes-0001";
        public const string NoteDoesNotExists = "notes-0002";
        public const string NoteTitleNotFound = "notes-0003";

        public const string ChecklistIdNotSupplied = "checklist-0001";
        public const string ChecklistDoesNotExists = "checklist-0002";

        public const string UserIdNotSupplied = "missing-uid";

        public const string EntityNotFound = "entry-not-found";
        public const string EntityDoesNotBelongToUser = "missing-permissions-over-entry";

        public const string ValidationsFailed = "validations-failed";

        public const string RepoProblem = "repo-error";     
    }
    public class ErrorDetail {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        // public string Description { get; set; }
    }

    public class ErrorStatusCode : Exception {
		public ErrorDetail Detail { get; set; }
		public int HttpStatusCode { get; set; }
		public ErrorStatusCode(int httpStatusCode, string errorCode,  string errorMessage) {
			Detail = new ErrorDetail {
				ErrorCode = errorCode,
				Message = errorMessage
			};
			HttpStatusCode = httpStatusCode;
		}
        public static readonly ErrorStatusCode UnknownError = new ErrorStatusCode(500, ErrorCodes.UnknownError, ErrorMessages.UnknownError);

        public static readonly ErrorStatusCode InvalidEmail = new ErrorStatusCode(400, ErrorCodes.InvalidEmail, ErrorMessages.InvalidEmail);
        public static readonly ErrorStatusCode InvalidPassword = new ErrorStatusCode(403, ErrorCodes.InvalidPassword, ErrorMessages.InvalidPassword);
        public static readonly ErrorStatusCode UserNameNotFound = new ErrorStatusCode(400, ErrorCodes.UserNameNotFound, ErrorMessages.UserNameNotFound);
        public static readonly ErrorStatusCode WeakPassWord = new ErrorStatusCode(400, ErrorCodes.WeakPassWord, ErrorMessages.WeakPassWord);
        public static readonly ErrorStatusCode PasswordsDoNotMatch = new ErrorStatusCode(400, ErrorCodes.PasswordsDoNotMatch, ErrorMessages.PasswordsDoNotMatch);
        public static readonly ErrorStatusCode EmailAlreadyInUse = new ErrorStatusCode(400, ErrorCodes.EmailAlreadyInUse, ErrorMessages.EmailAlreadyInUse);
        public static readonly ErrorStatusCode WrongPassword = new ErrorStatusCode(400, ErrorCodes.WrongPassword, ErrorMessages.WrongPassword);
        public static readonly ErrorStatusCode AuthorizarionMissing = new ErrorStatusCode(401, ErrorCodes.AuthorizarionMissing, ErrorMessages.AuthorizarionMissing);
        public static readonly ErrorStatusCode AuthorizationFailed = new ErrorStatusCode(401, ErrorCodes.AuthorizationFailed, ErrorMessages.AuthorizationFailed);
        public static readonly ErrorStatusCode UserNotFound = new ErrorStatusCode(404, ErrorCodes.UserNotFound, ErrorMessages.UserNotFound);
        public static readonly ErrorStatusCode NoEmail = new ErrorStatusCode(400, ErrorCodes.NoEmail, ErrorMessages.NoEmail);
        public static readonly ErrorStatusCode RefreshTokenExpired = new ErrorStatusCode(500, ErrorCodes.RefreshTokenExpired, ErrorMessages.RefreshTokenExpired);

        public static readonly ErrorStatusCode NoteIdNotSupplied = new ErrorStatusCode(400, ErrorCodes.NoteIdNotSupplied, ErrorMessages.NoteIdNotSupplied);
        public static readonly ErrorStatusCode NoteDoesNotExists = new ErrorStatusCode(404, ErrorCodes.NoteDoesNotExists, ErrorMessages.NoteDoesNotExists);
        public static readonly ErrorStatusCode NoteTitleNotFound = new ErrorStatusCode(400, ErrorCodes.NoteTitleNotFound, ErrorMessages.NoteTitleNotFound);

        public static readonly ErrorStatusCode ChecklistDoesNotExists = new ErrorStatusCode(404, ErrorCodes.ChecklistDoesNotExists, ErrorMessages.ChecklistDoesNotExists);

        public static readonly ErrorStatusCode UserIdNotSupplied = new ErrorStatusCode(400, ErrorCodes.UserIdNotSupplied, ErrorMessages.UserIdNotSupplied);

        public static readonly ErrorStatusCode EntityNotFound = new ErrorStatusCode(404, ErrorCodes.EntityNotFound, ErrorMessages.EntityNotFound);
        public static readonly ErrorStatusCode EntityDoesNotBelongToUser = new ErrorStatusCode(403, ErrorCodes.EntityDoesNotBelongToUser, ErrorMessages.EntityDoesNotBelongToUser);

        public static readonly ErrorStatusCode RepoProblem = new ErrorStatusCode(500, ErrorCodes.RepoProblem, ErrorMessages.RepoProblem);
    }
}
