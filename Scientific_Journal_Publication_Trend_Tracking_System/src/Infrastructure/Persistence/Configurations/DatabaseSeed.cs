using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Configurations;

internal static class DatabaseSeed
{
    internal static readonly Guid ComputerScienceTopicId =
        Guid.Parse("0bdaedb8-f741-4cf6-87c6-b82951db7d31");

    internal static readonly Guid ArtificialIntelligenceTopicId =
        Guid.Parse("8698ace8-d52d-48d6-b750-319684d0ea30");

    internal static readonly Guid OpenAlexDataSourceId =
        Guid.Parse("654c4415-64de-48f7-a0ea-094c00491f27");

    internal static readonly Guid WeeklyAiSyncJobId =
        Guid.Parse("8f95e2d4-86c3-41e2-ae16-7dba70d82747");

    private static readonly DateTime SeedTimestamp =
        new(2026, 6, 23, 0, 0, 0, DateTimeKind.Utc);

    internal static ResearchTopic[] ResearchTopics =>
    [
        new ResearchTopic
        {
            Id = ComputerScienceTopicId,
            Name = "Computer Science",
            Description = "Research related to computing, software, algorithms, and information systems.",
            Domain = ResearchDomain.ComputerScience,
            PapersCount = 0,
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        },
        new ResearchTopic
        {
            Id = ArtificialIntelligenceTopicId,
            Name = "Artificial Intelligence",
            Description = "Research related to artificial intelligence and intelligent systems.",
            Domain = ResearchDomain.ArtificialIntelligence,
            PapersCount = 0,
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        }
    ];

    internal static ApiDataSource OpenAlexDataSource => new()
    {
        Id = OpenAlexDataSourceId,
        SourceType = ApiSourceType.OpenAlex,
        Name = "OpenAlex",
        BaseUrl = "https://api.openalex.org",
        ApiKey = null,
        IsActive = true,
        LastSyncTime = DateTime.UnixEpoch,
        RequestsPerMinute = 100,
        CreatedAt = SeedTimestamp,
        UpdatedAt = SeedTimestamp
    };

    internal static SyncJob WeeklyAiSyncJob => new()
    {
        Id = WeeklyAiSyncJobId,
        JobName = "Weekly Artificial Intelligence Paper Sync",
        ApiDataSourceId = OpenAlexDataSourceId,
        Status = SyncJobStatus.Pending,
        SearchQuery = "Artificial Intelligence",
        BatchSize = 100,
        CronExpression = "0 0 * * 0",
        NextScheduledRun = null,
        LastRunTime = null,
        IsActive = true,
        RetryAttempts = 3,
        CreatedAt = SeedTimestamp,
        UpdatedAt = SeedTimestamp
    };
}
