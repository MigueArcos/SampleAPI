namespace ArchitectureTest.Domain.Errors;

public sealed record AppError(string Code, string? Message = null);
