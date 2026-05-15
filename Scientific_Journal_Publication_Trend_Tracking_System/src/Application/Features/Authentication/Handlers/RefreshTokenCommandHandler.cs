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
/// Handler for refresh token command
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenProvider _tokenProvider;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<LoginCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenProvider tokenProvider,
        IOptions<JwtOptions> jwtOptions,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenProvider = tokenProvider;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing token refresh");

        // Get user by refresh token
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken && !u.IsDeleted);

        if (user == null || user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            _logger.LogWarning("Token refresh failed: Invalid or expired refresh token");
            throw new UnauthorizedException(ValidationMessages.RefreshTokenInvalid);
        }

        // Get token history
        var tokenHistories = await _unitOfWork.RefreshTokenHistories.FindAsync(
            t => t.Token == request.RefreshToken && !t.IsRevoked,
            cancellationToken);

        var tokenHistory = tokenHistories.FirstOrDefault();
        if (tokenHistory == null)
        {
            _logger.LogWarning("Token refresh failed: Token history not found");
            throw new UnauthorizedException(ValidationMessages.RefreshTokenRevoked);
        }

        // Generate new tokens
        var newAccessToken = _tokenProvider.GenerateAccessToken(user);
        var newRefreshToken = _tokenProvider.GenerateRefreshToken();

        // Revoke old refresh token
        tokenHistory.IsRevoked = true;
        tokenHistory.RevokedAt = DateTime.UtcNow;
        tokenHistory.RevokeReason = "Token rotated during refresh";
        _unitOfWork.RefreshTokenHistories.Update(tokenHistory);

        // Update user with new tokens
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        // Log new token usage
        var newTokenHistory = new RefreshTokenHistory
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiryTime = user.RefreshTokenExpiryTime.Value,
            IsRevoked = false
        };

        _unitOfWork.Users.Update(user);
        await _unitOfWork.RefreshTokenHistories.AddAsync(newTokenHistory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpirationMinutes = _jwtOptions.AccessTokenExpirationMinutes
        };
    }
}
