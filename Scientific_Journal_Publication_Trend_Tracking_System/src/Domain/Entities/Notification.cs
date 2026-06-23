using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents a notification sent to users
/// </summary>
public class Notification : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedPaperId { get; set; }
    public Guid? RelatedJournalId { get; set; }
    public Guid? RelatedResearchTopicId { get; set; }
    public bool IsRead { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ResearchPaper? RelatedPaper { get; set; }
    public Journal? RelatedJournal { get; set; }
    public ResearchTopic? RelatedResearchTopic { get; set; }
}

public enum NotificationType
{
    NewPaperPublished = 0,
    TrendingTopicAlert = 1,
    JournalUpdate = 2,
    SystemNotification = 3
}
