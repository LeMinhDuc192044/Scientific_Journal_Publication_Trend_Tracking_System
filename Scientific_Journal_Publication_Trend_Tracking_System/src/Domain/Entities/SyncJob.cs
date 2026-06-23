using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Represents a background synchronization job
/// </summary>
public class SyncJob : IAuditableEntity
{
    public Guid Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public Guid ApiDataSourceId { get; set; }
    public SyncJobStatus Status { get; set; }
    public string SearchQuery { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 100;
    public string? CronExpression { get; set; }
    public DateTime? NextScheduledRun { get; set; }
    public DateTime? LastRunTime { get; set; }
    public bool IsActive { get; set; } = true;
    public int RetryAttempts { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ApiDataSource ApiDataSource { get; set; } = null!;
    public ICollection<SyncLog> SyncLogs { get; set; } = new List<SyncLog>();
}

public enum SyncJobStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Paused = 4
}
