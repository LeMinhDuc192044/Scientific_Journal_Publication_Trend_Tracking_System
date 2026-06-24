using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents a user bookmark for papers, keywords, journals, or topics
/// </summary>
public class Bookmark : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public BookmarkType Type { get; set; }
    public Guid? ResearchPaperId { get; set; }
    public Guid? KeywordId { get; set; }
    public Guid? JournalId { get; set; }
    public Guid? ResearchTopicId { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ResearchPaper? ResearchPaper { get; set; }
    public Keyword? Keyword { get; set; }
    public Journal? Journal { get; set; }
    public ResearchTopic? ResearchTopic { get; set; }
}

public enum BookmarkType
{
    Paper = 0,
    Journal = 1,
    ResearchTopic = 2,
    Keyword = 3
}
