using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Commands;

/// <summary>
/// Command for refreshing access token
/// </summary>
public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;
