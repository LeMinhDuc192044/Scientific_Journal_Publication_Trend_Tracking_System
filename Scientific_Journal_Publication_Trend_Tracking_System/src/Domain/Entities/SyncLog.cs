namespace Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

/// <summary>
/// Logs the execution of synchronization jobs
/// </summary>
public class SyncLog : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid SyncJobId { get; set; }
    public Guid ApiDataSourceId { get; set; }
    public DateTime ExecutionStartTime { get; set; }
    public DateTime? ExecutionEndTime { get; set; }
    public bool IsSuccessful { get; set; }
    public int RecordsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public SyncJob SyncJob { get; set; } = null!;
    public ApiDataSource ApiDataSource { get; set; } = null!;
}
