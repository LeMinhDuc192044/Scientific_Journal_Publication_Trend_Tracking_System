using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.Commands;

public sealed record CreateFollowSubscriptionCommand(
    FollowTargetType TargetType,
    Guid TargetId) : IRequest<CreateFollowSubscriptionResult>;

public sealed record UnfollowSubscriptionCommand(Guid SubscriptionId) : IRequest;
