using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Authentication;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Constants;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;
using Microsoft.Extensions.Options;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Handlers;

/// <summary>
/// Handler for user login command
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _tokenProvider;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenProvider tokenProvider,
        IOptions<JwtOptions> jwtOptions,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenProvider = tokenProvider;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing login for email: {Email}", request.Email);

        // Get user by email
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
            throw new UnauthorizedException(ValidationMessages.InvalidCredentials);
        }

        // Verify password asynchronously to offload from request thread
        var passwordValid = _passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            _logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);
            throw new UnauthorizedException(ValidationMessages.InvalidCredentials);
        }

        // Generate tokens
        var accessToken = _tokenProvider.GenerateAccessToken(user);
        var refreshToken = _tokenProvider.GenerateRefreshToken();

        // Save refresh token
        // Update user
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime =
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        _unitOfWork.Users.Update(user);

        // Add refresh token history
        var tokenHistory = new RefreshTokenHistory
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiryTime = user.RefreshTokenExpiryTime.Value,
            IsRevoked = false
        };

        await _unitOfWork.RefreshTokenHistories.AddAsync(tokenHistory, cancellationToken);

        // SINGLE SAVE
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        return new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpirationMinutes = _jwtOptions.AccessTokenExpirationMinutes
        };
    }
}
