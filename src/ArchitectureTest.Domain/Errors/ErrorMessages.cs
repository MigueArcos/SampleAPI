namespace ArchitectureTest.Domain.Errors;

public static class ErrorMessages 
{
    public const string DbTransactionError = "An exception occurred during DB transaction";
    public const string DefaultErrorMessageForExceptions = "An exception occurred";
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
