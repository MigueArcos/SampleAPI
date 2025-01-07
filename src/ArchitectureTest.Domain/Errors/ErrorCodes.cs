namespace ArchitectureTest.Domain.Errors;

public struct ErrorCodes {
    public const string UnknownError = "unknown-error";
    public const string InvalidEmail = "auth-invalid-email";
    public const string InvalidPassword = "auth-invalid-password";
    public const string UserNameNotFound = "auth-username-not-found";
    public const string WeakPassWord = "auth-weak-password";
    public const string CannotGenerateJwtToken = "auth-cannot-generate-token";
    public const string IncompleteJwtTokenData = "auth-incomplete-token-data";
    public const string PasswordsDoNotMatch = "auth-passwords-mismatch";
    public const string EmailAlreadyInUse = "auth-email-in-use";
    public const string WrongPassword = "auth-wrong-password";
    public const string AuthorizarionMissing = "authorization-missing";
    public const string AuthorizationFailed = "authorization-failed";
    public const string UserNotFound = "auth-user-not-found";
    public const string NoEmail = "auth-no-email";
    public const string InvalidUserName = "auth-invalid-username";
    public const string RefreshTokenExpired = "auth-refresh-token-expired";
    public const string NoteIdNotSupplied = "note-id-not-supplied";
    public const string NoteDoesNotExists = "note-does-not-exists";
    public const string NoteTitleNotFound = "note-title-not-found";
    public const string ChecklistTitleNotFound = "checklist-title-not-found";
    public const string ChecklistIdNotSupplied = "checklist-id-not-supplied";
    public const string ChecklistDoesNotExists = "checklis-does-not-exists";
    public const string UserIdNotSupplied = "missing-uid";
    public const string EntityNotFound = "data-not-found";
    public const string InputDataNotFound = "input-data-not-found";
    public const string IncorrectInputData = "incorrect-input-data";
    public const string EntityDoesNotBelongToUser = "missing-permissions-over-entry";
    public const string CannotCreateDataForThisUserId = "cannot-create-data-for-this-user-id";
    public const string ValidationsFailed = "validations-failed";
    public const string RepoProblem = "repo-error";
    public const string OneOrMoreChecklistDetailToUpdateNotFound = "one-or-more-checklist-detail-to-update-does-not-exists"; 
    public const string OneOrMoreChecklistDetailToDeleteNotFound = "one-or-more-checklist-detail-to-delete-does-not-exists"; 
}
