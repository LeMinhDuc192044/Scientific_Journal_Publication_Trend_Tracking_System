using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents a user's subscription to a journal or research topic.
/// A subscription is deactivated on unfollow so it can be reactivated later.
/// </summary>
public class FollowSubscription : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public FollowTargetType Type { get; set; }
    public Guid? JournalId { get; set; }
    public Guid? ResearchTopicId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Journal? Journal { get; set; }
    public ResearchTopic? ResearchTopic { get; set; }
}
