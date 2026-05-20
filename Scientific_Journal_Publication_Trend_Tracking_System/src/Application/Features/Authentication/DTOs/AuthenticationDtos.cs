namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

/// <summary>
/// DTO for user registration request
/// </summary>
public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public UserRole Role { get; init; } = UserRole.User;
}

/// <summary>
/// DTO for user registration response
/// </summary>
public record RegisterResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Message { get; init; } = "User registered successfully";
}

/// <summary>
/// DTO for user login request
/// </summary>
public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// DTO for user login response with tokens
/// </summary>
public record LoginResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; init; }
}

/// <summary>
/// DTO for refresh token request
/// </summary>
public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// DTO for refresh token response
/// </summary>
public record RefreshTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; init; }
}
