namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents a research keyword for paper tagging and searching
/// </summary>
public class Keyword : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FrequencyCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
    public ICollection<ResearchTopic> ResearchTopics { get; set; } = new List<ResearchTopic>();
}
