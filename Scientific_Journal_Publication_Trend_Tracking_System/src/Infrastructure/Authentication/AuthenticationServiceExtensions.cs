using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Authentication;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Constants;
using System.Text;
using System.Text.Json;


namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.Authentication;


public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── JWT options binding ─────────────────────────────────────────────────
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration is missing. Ensure 'Jwt' section exists in appsettings.json");

        var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

        // ── Authentication scheme + token validation ────────────────────────────
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = jwtOptions.ValidateLifetime,
                ClockSkew = TimeSpan.FromSeconds(jwtOptions.ClockSkewSeconds)
            };

            // Custom 401/403 bodies — by default ASP.NET Core returns these empty.
            options.Events = new JwtBearerEvents
            {
                // Captures the REAL reason validation failed (bad signature, wrong
                // issuer/audience, expired, etc.) so OnChallenge below can report it.
                OnAuthenticationFailed = context =>
                {
                    context.HttpContext.Items["JwtAuthError"] =
                        $"{context.Exception.GetType().Name}: {context.Exception.Message}";
                    return Task.CompletedTask;
                },

                // Missing token, malformed token, expired token, bad signature, etc.
                OnChallenge = async context =>
                {
                    context.HandleResponse(); // suppress the default empty-body 401

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    // TEMP DEBUG FIELD — shows the exact validation failure.
                    // Remove the "debug" property before shipping to production.
                    var debugReason = context.HttpContext.Items["JwtAuthError"] as string;

                    var payload = JsonSerializer.Serialize(new
                    {
                        success = false,
                        errorCode = ErrorCodes.InvalidCredentials, // swap for a dedicated "UNAUTHORIZED" code if added
                        message = "You are unauthorized. Please provide a valid access token.",
                        debug = debugReason
                    });

                    await context.Response.WriteAsync(payload);
                },

                // Token IS valid but fails an [Authorize(Roles/Policy = ...)] check
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var payload = JsonSerializer.Serialize(new
                    {
                        success = false,
                        errorCode = "FORBIDDEN",
                        message = "You do not have permission to access this resource."
                    });

                    await context.Response.WriteAsync(payload);
                }
            };
        });

        // ── Authorization services ──────────────────────────────────────────────
        services.AddAuthorization();

        return services;
    }
}

