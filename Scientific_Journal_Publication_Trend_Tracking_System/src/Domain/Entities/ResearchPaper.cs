using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents metadata for a scientific research paper
/// </summary>
public class ResearchPaper : IAuditableEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string ApiSource { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Abstract { get; set; }
    public int PublicationYear { get; set; }
    public string? Doi { get; set; } 
    public Guid? JournalId { get; set; }
    public string? Url { get; set; }
    public int CitationCount { get; set; }
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public ResearchDomain Domain { get; set; }
    public bool IsFullTextAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Journal? Journal { get; set; }
    public ICollection<Author> Authors { get; set; } = new List<Author>();
    public ICollection<Keyword> ResearchKeywords { get; set; } = new List<Keyword>();
    public ICollection<ResearchTopic> ResearchTopics { get; set; } = new List<ResearchTopic>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<PublicationTrend> PublicationTrends { get; set; } = new List<PublicationTrend>();
}
