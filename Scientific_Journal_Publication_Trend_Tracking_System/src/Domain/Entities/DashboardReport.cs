namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents dashboard analytics and reports
/// </summary>
public class DashboardReport : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TotalPapersCount { get; set; }
    public int ActiveUsersCount { get; set; }
    public string TopResearchDomains { get; set; } = string.Empty;
    public string MostCitedPapers { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
