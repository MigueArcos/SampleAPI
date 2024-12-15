using ArchitectureTest.Domain.Models.Enums;

namespace ArchitectureTest.Web.Configuration;

public static class HttpResponses
{
    public static readonly IDictionary<string, HttpErrorInfo> CommonErrors = new Dictionary<string, HttpErrorInfo>{
        [ErrorCodes.UnknownError] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status500InternalServerError,
            ErrorCode = ErrorCodes.UnknownError,
            Message = ErrorMessages.UnknownError
        },
        [ErrorCodes.InvalidEmail] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.InvalidEmail,
            Message = ErrorMessages.InvalidEmail
        },
        [ErrorCodes.InvalidPassword] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status403Forbidden,
            ErrorCode = ErrorCodes.InvalidPassword,
            Message = ErrorMessages.InvalidPassword
        },
        [ErrorCodes.UserNameNotFound] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.UserNameNotFound,
            Message = ErrorMessages.UserNameNotFound
        },
        [ErrorCodes.WeakPassWord] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.WeakPassWord,
            Message = ErrorMessages.WeakPassWord
        },
        [ErrorCodes.PasswordsDoNotMatch] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.PasswordsDoNotMatch,
            Message = ErrorMessages.PasswordsDoNotMatch
        },
        [ErrorCodes.CannotGenerateJwtToken] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status500InternalServerError,
            ErrorCode = ErrorCodes.CannotGenerateJwtToken
        },
        [ErrorCodes.IncompleteJwtTokenData] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.IncompleteJwtTokenData
        },
        [ErrorCodes.EmailAlreadyInUse] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.EmailAlreadyInUse,
            Message = ErrorMessages.EmailAlreadyInUse
        },
        [ErrorCodes.WrongPassword] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.WrongPassword,
            Message = ErrorMessages.WrongPassword
        },
        [ErrorCodes.AuthorizarionMissing] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status401Unauthorized,
            ErrorCode = ErrorCodes.AuthorizarionMissing,
            Message = ErrorMessages.AuthorizarionMissing
        },
        [ErrorCodes.AuthorizationFailed] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status401Unauthorized,
            ErrorCode = ErrorCodes.AuthorizationFailed,
            Message = ErrorMessages.AuthorizationFailed
        },
        [ErrorCodes.UserNotFound] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status404NotFound,
            ErrorCode = ErrorCodes.UserNotFound,
            Message = ErrorMessages.UserNotFound
        },
        [ErrorCodes.NoEmail] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.NoEmail,
            Message = ErrorMessages.NoEmail
        },
        [ErrorCodes.RefreshTokenExpired] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status500InternalServerError,
            ErrorCode = ErrorCodes.RefreshTokenExpired,
            Message = ErrorMessages.RefreshTokenExpired
        },
        [ErrorCodes.NoteIdNotSupplied] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.NoteIdNotSupplied,
            Message = ErrorMessages.NoteIdNotSupplied
        },
        [ErrorCodes.NoteDoesNotExists] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status404NotFound,
            ErrorCode = ErrorCodes.NoteDoesNotExists,
            Message = ErrorMessages.NoteDoesNotExists
        },
        [ErrorCodes.NoteTitleNotFound] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.NoteTitleNotFound,
            Message = ErrorMessages.NoteTitleNotFound
        },
        [ErrorCodes.ChecklistIdNotSupplied] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.ChecklistIdNotSupplied,
            Message = ErrorMessages.ChecklistIdNotSupplied
        },
        [ErrorCodes.ChecklistDoesNotExists] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status404NotFound,
            ErrorCode = ErrorCodes.ChecklistDoesNotExists,
            Message = ErrorMessages.ChecklistDoesNotExists
        },
        [ErrorCodes.UserIdNotSupplied] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.UserIdNotSupplied,
            Message = ErrorMessages.UserIdNotSupplied
        },
        [ErrorCodes.EntityNotFound] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status404NotFound,
            ErrorCode = ErrorCodes.EntityNotFound,
            Message = ErrorMessages.EntityNotFound
        },
        [ErrorCodes.InputDataNotFound] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.InputDataNotFound,
        },
        [ErrorCodes.EntityDoesNotBelongToUser] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status403Forbidden,
            ErrorCode = ErrorCodes.EntityDoesNotBelongToUser,
            Message = ErrorMessages.EntityDoesNotBelongToUser
        },
        [ErrorCodes.CannotCreateDataForThisUserId] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status403Forbidden,
            ErrorCode = ErrorCodes.CannotCreateDataForThisUserId,
        },
        [ErrorCodes.RepoProblem] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status500InternalServerError,
            ErrorCode = ErrorCodes.RepoProblem,
            Message = ErrorMessages.RepoProblem
        },
        [ErrorCodes.IncorrectInputData] = new HttpErrorInfo
        {
            HttpStatusCode = StatusCodes.Status400BadRequest,
            ErrorCode = ErrorCodes.IncorrectInputData
        }
    };

    public static HttpErrorInfo TryGetErrorInfo(string errorMessage, Action<string> onUnknownErrorFound){
        var isAManagedError = CommonErrors.TryGetValue(errorMessage, out var errorInfo);
        if (!isAManagedError) {
            // We should never expose real exceptions, so we will catch all unknown exceptions 
            // (DatabaseErrors, Null Errors, Index errors, etc...) and rethrow an UnknownError after log
            errorInfo = CommonErrors[ErrorCodes.UnknownError];
            onUnknownErrorFound(errorMessage);
        }
        return errorInfo!;
    }
}
