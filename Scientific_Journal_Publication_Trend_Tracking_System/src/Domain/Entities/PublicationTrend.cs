namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents statistical trends for publication analysis
/// </summary>
public class PublicationTrend : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ResearchTopicId { get; set; }
    public Guid? KeywordId { get; set; }
    public Guid? JournalId { get; set; }
    public int Year { get; set; }
    public int PublicationCount { get; set; }
    public double AverageCitations { get; set; }
    public double GrowthRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ResearchTopic ResearchTopic { get; set; } = null!;
    public Keyword? Keyword { get; set; } 
    public Journal? Journal { get; set; }
}
