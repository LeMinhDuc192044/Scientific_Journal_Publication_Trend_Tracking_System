using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents external API data source configuration
/// </summary>
public class ApiDataSource : IAuditableEntity
{
    public Guid Id { get; set; }
    public ApiSourceType SourceType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime LastSyncTime { get; set; }
    public int RequestsPerMinute { get; set; } = 100;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<SyncLog> SyncLogs { get; set; } = new List<SyncLog>();
    public ICollection<SyncJob> SyncJobs { get; set; } = new List<SyncJob>();
}

public enum ApiSourceType
{
    SemanticScholar = 0,
    OpenAlex = 1,
    Crossref = 2
}
