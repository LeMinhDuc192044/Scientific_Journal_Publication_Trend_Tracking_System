using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents a research topic for categorization and trend analysis
/// </summary>
public class ResearchTopic : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ResearchDomain Domain { get; set; }
    public int PapersCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
    public ICollection<Keyword> Keywords { get; set; } = new List<Keyword>();
    public ICollection<PublicationTrend> PublicationTrends { get; set; } = new List<PublicationTrend>();
}
