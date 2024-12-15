namespace ArchitectureTest.Domain.Models;

public sealed record AppError(string Code, string? Message = null);
