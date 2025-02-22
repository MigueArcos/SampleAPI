﻿namespace ArchitectureTest.Domain.Models.Application;

public class JsonWebToken {
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public required long ExpiresIn { get; set; }
}
