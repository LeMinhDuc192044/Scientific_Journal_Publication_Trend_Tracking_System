using System.ComponentModel.DataAnnotations;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Authentication;

/// <summary>
/// Configuration options for JWT token generation and validation
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "Jwt:SecretKey is required")]
    public string SecretKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jwt:Issuer is required")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jwt:Audience is required")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    [Range(1, 10080)]
    public int RefreshTokenExpirationDays { get; set; } = 7;

    public bool ValidateIssuerSigningKey { get; set; } = true;

    public bool ValidateIssuer { get; set; } = true;

    public bool ValidateAudience { get; set; } = true;

    public bool ValidateLifetime { get; set; } = true;

    public int ClockSkewSeconds { get; set; } = 5;
}
