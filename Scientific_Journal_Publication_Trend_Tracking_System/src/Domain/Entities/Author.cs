namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents an author of research papers
/// </summary>
public class Author : IAuditableEntity
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Orcid { get; set; }
    public int PublicationCount { get; set; }
    public string? HomepageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
}
