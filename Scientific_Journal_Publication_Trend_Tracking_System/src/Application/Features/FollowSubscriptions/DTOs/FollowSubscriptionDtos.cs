using System.Text.Json.Serialization;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.DTOs;

public sealed record CreateFollowSubscriptionRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FollowTargetType TargetType { get; init; }

    public Guid TargetId { get; init; }
}

public sealed record FollowSubscriptionDto
{
    public Guid Id { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FollowTargetType TargetType { get; init; }

    public Guid TargetId { get; init; }
    public string TargetName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed record CreateFollowSubscriptionResult(
    FollowSubscriptionDto Subscription,
    bool Created,
    bool Reactivated);
