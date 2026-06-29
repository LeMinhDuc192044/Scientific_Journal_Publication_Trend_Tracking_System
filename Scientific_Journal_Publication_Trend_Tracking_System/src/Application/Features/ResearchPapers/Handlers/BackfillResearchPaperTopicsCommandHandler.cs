using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Services;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Handlers;

public sealed class BackfillResearchPaperTopicsCommandHandler
    : IRequestHandler<BackfillResearchPaperTopicsCommand, BackfillResearchPaperTopicsResult>
{
    private const int SampleLimit = 10;

    private readonly AppDbContext _dbContext;
    private readonly IResearchTopicMatcher _topicMatcher;
    private readonly ILogger<BackfillResearchPaperTopicsCommandHandler> _logger;

    public BackfillResearchPaperTopicsCommandHandler(
        AppDbContext dbContext,
        IResearchTopicMatcher topicMatcher,
        ILogger<BackfillResearchPaperTopicsCommandHandler> logger)
    {
        _dbContext = dbContext;
        _topicMatcher = topicMatcher;
        _logger = logger;
    }

    public async Task<BackfillResearchPaperTopicsResult> Handle(
        BackfillResearchPaperTopicsCommand request,
        CancellationToken cancellationToken)
    {
        var topics = await _dbContext.ResearchTopics
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        if (topics.Count == 0)
        {
            return new BackfillResearchPaperTopicsResult(
                DryRun: request.DryRun,
                ProcessedPapers: 0,
                PapersMatched: 0,
                PapersSkipped: 0,
                LinksCreated: 0,
                TopicsUpdated: 0,
                MatchedSamples: [],
                UnmatchedSamples: []);
        }

        var query = _dbContext.ResearchPapers
            .Include(p => p.ResearchTopics)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        if (request.OnlyWithoutTopics)
        {
            query = query.Where(p => !p.ResearchTopics.Any());
        }

        var papers = await query
            .Take(request.BatchSize)
            .ToListAsync(cancellationToken);

        var processed = 0;
        var matched = 0;
        var skipped = 0;
        var linksCreated = 0;
        var matchedSamples = new List<BackfilledPaperTopicDto>();
        var unmatchedSamples = new List<UnmatchedPaperDto>();

        foreach (var paper in papers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            processed++;

            var existingTopicIds = paper.ResearchTopics
                .Select(t => t.Id)
                .ToHashSet();

            var matchedTopics = _topicMatcher
                .MatchTopics(paper, topics, request.MaxTopicsPerPaper)
                .Where(t => !existingTopicIds.Contains(t.Id))
                .ToList();

            if (matchedTopics.Count == 0)
            {
                skipped++;

                if (unmatchedSamples.Count < SampleLimit)
                {
                    unmatchedSamples.Add(new UnmatchedPaperDto(paper.Id, paper.Title));
                }

                continue;
            }

            foreach (var topic in matchedTopics)
            {
                if (!request.DryRun)
                {
                    paper.ResearchTopics.Add(topic);
                }

                linksCreated++;
            }

            if (!request.DryRun)
            {
                paper.UpdatedAt = DateTime.UtcNow;
            }

            matched++;

            if (matchedSamples.Count < SampleLimit)
            {
                matchedSamples.Add(new BackfilledPaperTopicDto(
                    paper.Id,
                    paper.Title,
                    matchedTopics.Select(t => t.Name).ToList()));
            }
        }

        var topicsUpdated = 0;

        if (!request.DryRun)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            topicsUpdated = await RefreshTopicPaperCountsAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Paper topic backfill completed. DryRun={DryRun}, Processed={Processed}, Matched={Matched}, Skipped={Skipped}, LinksCreated={LinksCreated}, TopicsUpdated={TopicsUpdated}",
            request.DryRun,
            processed,
            matched,
            skipped,
            linksCreated,
            topicsUpdated);

        return new BackfillResearchPaperTopicsResult(
            request.DryRun,
            processed,
            matched,
            skipped,
            linksCreated,
            topicsUpdated,
            matchedSamples,
            unmatchedSamples);
    }

    private async Task<int> RefreshTopicPaperCountsAsync(CancellationToken cancellationToken)
    {
        var topicCounts = await _dbContext.ResearchTopics
            .Select(t => new
            {
                t.Id,
                PapersCount = t.ResearchPapers.Count
            })
            .ToListAsync(cancellationToken);

        var countsByTopicId = topicCounts.ToDictionary(x => x.Id, x => x.PapersCount);
        var topicsUpdated = 0;

        var topics = await _dbContext.ResearchTopics
            .ToListAsync(cancellationToken);

        foreach (var topic in topics)
        {
            if (!countsByTopicId.TryGetValue(topic.Id, out var papersCount) ||
                topic.PapersCount == papersCount)
            {
                continue;
            }

            topic.PapersCount = papersCount;
            topic.UpdatedAt = DateTime.UtcNow;
            topicsUpdated++;
        }

        if (topicsUpdated > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return topicsUpdated;
    }
}
