namespace PropertyViewerAccounting.Core.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, string RefreshToken, DateTime ExpiresAt, UserDto User);

public record RefreshTokenRequest(string RefreshToken);

public record UserDto(Guid Id, string Email, string? FirstName, string? LastName, string Role);
