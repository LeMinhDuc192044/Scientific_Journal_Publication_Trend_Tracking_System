using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.Queries;

public sealed record GetMyFollowSubscriptionsQuery(
    FollowTargetType? TargetType,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<FollowSubscriptionDto>>;
