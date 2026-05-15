using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Commands;

/// <summary>
/// Command for user login
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
