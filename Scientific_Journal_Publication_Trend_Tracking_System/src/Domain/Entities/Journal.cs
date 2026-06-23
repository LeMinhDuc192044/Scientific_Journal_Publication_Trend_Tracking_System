namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents an academic journal
/// </summary>
public class Journal : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? IssueNumber { get; set; }
    public string? Issn { get; set; }
    public string? Publisher { get; set; }
    public int EstablishedYear { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public int TotalPapersPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<PublicationTrend> PublicationTrends { get; set; } = new List<PublicationTrend>();
    public ICollection<FollowSubscription> FollowSubscriptions { get; set; } = new List<FollowSubscription>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
