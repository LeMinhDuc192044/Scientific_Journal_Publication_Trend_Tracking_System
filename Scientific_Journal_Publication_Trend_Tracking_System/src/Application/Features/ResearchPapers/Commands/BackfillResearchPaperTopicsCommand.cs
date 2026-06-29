using MediatR;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Commands;

/// <summary>
/// Backfills the PaperTopics many-to-many table for existing research papers.
/// </summary>
public sealed record BackfillResearchPaperTopicsCommand(
    bool OnlyWithoutTopics = true,
    int MaxTopicsPerPaper = 3,
    int BatchSize = 1000,
    bool DryRun = false) : IRequest<BackfillResearchPaperTopicsResult>;

public sealed record BackfillResearchPaperTopicsResult(
    bool DryRun,
    int ProcessedPapers,
    int PapersMatched,
    int PapersSkipped,
    int LinksCreated,
    int TopicsUpdated,
    IReadOnlyList<BackfilledPaperTopicDto> MatchedSamples,
    IReadOnlyList<UnmatchedPaperDto> UnmatchedSamples);

public sealed record BackfilledPaperTopicDto(
    Guid PaperId,
    string PaperTitle,
    IReadOnlyList<string> Topics);

public sealed record UnmatchedPaperDto(
    Guid PaperId,
    string PaperTitle);
