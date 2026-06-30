using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Services;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.ExternalApis.ExternalPaperDto;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchPapers.Handlers;

/// <summary>
/// Refreshes existing research papers in the database with the latest metadata
/// from external APIs (title, abstract, keywords, year, citationCount, authors, journal).
///
/// - Papers that exist in the DB (matched by ExternalId + ApiSource) → UPDATED
/// - Papers returned by the API that are not yet in the DB → CREATED
/// - Full-text is never fetched (copyright + storage constraints)
/// </summary>
public class SyncResearchPapersCommandHandler
    : IRequestHandler<SyncResearchPapersCommand, SyncPapersResponse>
{
    private readonly IExternalApiSyncService _externalApiService;
    private readonly AppDbContext _dbContext;
    private readonly IResearchTopicMatcher _topicMatcher;
    private readonly ILogger<SyncResearchPapersCommandHandler> _logger;

    public SyncResearchPapersCommandHandler(
        IExternalApiSyncService externalApiService,
        AppDbContext dbContext,
        IResearchTopicMatcher topicMatcher,
        ILogger<SyncResearchPapersCommandHandler> logger)
    {
        _externalApiService = externalApiService;
        _dbContext = dbContext;
        _topicMatcher = topicMatcher;
        _logger = logger;
    }

    public async Task<SyncPapersResponse> Handle(
        SyncResearchPapersCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var created = 0;
        var updated = 0;
        var topicLinksCreated = 0;

        _logger.LogInformation(
            "Starting sync for query: '{Query}' from {ApiSource}",
            request.Query, request.ApiSource);

        // ── 1. Fetch latest metadata from external API ────────────────────────
        IReadOnlyList<ExternalPaperDto> papers;
        try
        {
            papers = request.ApiSource.ToLowerInvariant() switch
            {
                "semanticscholar" => await _externalApiService.SearchSemanticScholarAsync(
                                         request.Query, request.Limit, cancellationToken),
                "openalex" => await _externalApiService.SearchOpenAlexAsync(
                                         request.Query, request.Limit, cancellationToken),
                "crossref" => await _externalApiService.SearchCrossrefAsync(
                                         request.Query, request.Limit, cancellationToken),
                _ => throw new ArgumentException(
                         $"Unsupported API source: '{request.ApiSource}'. " +
                         "Valid values: semanticscholar, openalex, crossref")
            };
        }
        catch (HttpRequestException ex)
        {
            var msg = $"External API request failed [{request.ApiSource}]: {ex.Message}";
            _logger.LogError(ex, msg);
            return new SyncPapersResponse(0, 0, 0, [msg]);
        }
        catch (Exception ex)
        {
            var msg = $"Failed to fetch/parse response from [{request.ApiSource}]: {ex.GetType().Name} — {ex.Message}";
            _logger.LogError(ex, msg);
            return new SyncPapersResponse(0, 0, 0, [msg]);
        }

        _logger.LogInformation("Fetched {Count} papers from {Source}",
            papers.Count, request.ApiSource);

        var topics = await _dbContext.ResearchTopics
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var paperIds = papers
            .Select(p => p.PaperId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var existingPapers = await _dbContext.ResearchPapers
            .Include(p => p.Authors)
            .Include(p => p.Journal)
            .Include(p => p.ResearchTopics)
            .Where(p => p.ApiSource == request.ApiSource && paperIds.Contains(p.ExternalId))
            .ToListAsync(cancellationToken);

        var existingPapersByExternalId = existingPapers
            .GroupBy(p => p.ExternalId)
            .ToDictionary(g => g.Key, g => g.First());

        // ── 2. Upsert each paper returned by the external API ────────────────
        foreach (var paperDto in papers)
        {
            try
            {
                if (!existingPapersByExternalId.TryGetValue(paperDto.PaperId, out var existing))
                {
                    var newPaper = await BuildPaperAsync(
                        paperDto,
                        request.ApiSource,
                        topics,
                        cancellationToken);

                    topicLinksCreated += newPaper.ResearchTopics.Count;
                    _dbContext.ResearchPapers.Add(newPaper);
                    existingPapersByExternalId[paperDto.PaperId] = newPaper;
                    created++;
                    continue;
                }

                // ── Update scalar metadata ────────────────────────────────────
                existing.Title = paperDto.Title;
                existing.Abstract = paperDto.Abstract;
                existing.Keywords = paperDto.Keywords ?? existing.Keywords;
                existing.PublicationYear = paperDto.Year ?? existing.PublicationYear;
                existing.Doi = paperDto.Doi ?? existing.Doi;
                existing.Url = paperDto.Url ?? existing.Url;
                existing.CitationCount = paperDto.CitationCount ?? existing.CitationCount;
                existing.UpdatedAt = DateTime.UtcNow;

                // ── Update journal ────────────────────────────────────────────
                if (!string.IsNullOrWhiteSpace(paperDto.JournalName))
                    existing.Journal = await GetOrAddJournalAsync(paperDto.JournalName, cancellationToken);

                // ── Update authors ────────────────────────────────────────────
                // Strategy: add any new authors from the API that aren't already
                // linked to this paper. Existing links are preserved.
                if (paperDto.Authors is { Count: > 0 })
                {
                    foreach (var authorDto in paperDto.Authors)
                    {
                        var author = await GetOrAddAuthorAsync(authorDto, cancellationToken);

                        var alreadyLinked = existing.Authors
                            .Any(a => a.Id == author.Id);

                        if (!alreadyLinked)
                            existing.Authors.Add(author);
                    }
                }

                var existingTopicIds = existing.ResearchTopics
                    .Select(t => t.Id)
                    .ToHashSet();

                var matchedTopics = _topicMatcher
                    .MatchTopics(existing, topics, maxTopics: 3)
                    .Where(t => !existingTopicIds.Contains(t.Id))
                    .ToList();

                foreach (var topic in matchedTopics)
                {
                    existing.ResearchTopics.Add(topic);
                    topic.PapersCount++;
                    topic.UpdatedAt = DateTime.UtcNow;
                    topicLinksCreated++;
                }

                updated++;
            }
            catch (Exception ex)
            {
                var msg = $"Error syncing paper '{paperDto.Title}': {ex.Message}";
                _logger.LogError(ex, msg);
                errors.Add(msg);
            }
        }

        // ── 3. Persist all updates ────────────────────────────────────────────
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Sync complete — created: {C}, updated: {U}, topic links created: {T}, errors: {E}",
            created, updated, topicLinksCreated, errors.Count);

        return new SyncPapersResponse(created + updated, created, updated, errors, topicLinksCreated);
    }

    private async Task<ResearchPaper> BuildPaperAsync(
        ExternalPaperDto dto,
        string apiSource,
        IReadOnlyCollection<ResearchTopic> topics,
        CancellationToken ct)
    {
        var paper = new ResearchPaper
        {
            Id = Guid.NewGuid(),
            ExternalId = dto.PaperId,
            ApiSource = apiSource,
            Title = dto.Title,
            Abstract = dto.Abstract,
            Keywords = dto.Keywords ?? [],
            PublicationYear = dto.Year ?? DateTime.UtcNow.Year,
            Doi = dto.Doi,
            Url = dto.Url,
            CitationCount = dto.CitationCount ?? 0,
            Domain = InferDomain(dto.Keywords, dto.Title),
            IsFullTextAvailable = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(dto.JournalName))
        {
            paper.Journal = await GetOrAddJournalAsync(dto.JournalName, ct);
        }

        if (dto.Authors is { Count: > 0 })
        {
            foreach (var authorDto in dto.Authors)
            {
                paper.Authors.Add(await GetOrAddAuthorAsync(authorDto, ct));
            }
        }

        foreach (var topic in _topicMatcher.MatchTopics(paper, topics, maxTopics: 3))
        {
            paper.ResearchTopics.Add(topic);
            topic.PapersCount++;
            topic.UpdatedAt = DateTime.UtcNow;
        }

        return paper;
    }

    // ── Journal: reuse by title, create if missing ────────────────────────────

    private async Task<Journal> GetOrAddJournalAsync(string name, CancellationToken ct)
    {
        var normalised = name.Trim().ToLower();

        // 1. Already in DB
        var journal = await _dbContext.Journals
            .FirstOrDefaultAsync(j => j.Title.ToLower() == normalised, ct);
        if (journal is not null) return journal;

        // 2. Added earlier in this same batch (not saved yet)
        journal = _dbContext.Journals.Local
            .FirstOrDefault(j => j.Title.ToLower() == normalised);
        if (journal is not null) return journal;

        // 3. Create new
        journal = new Journal
        {
            Id = Guid.NewGuid(),
            Title = name.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Journals.Add(journal);
        return journal;
    }

    // ── Author: reuse by ORCID → full name, create if missing ────────────────

    private async Task<Author> GetOrAddAuthorAsync(ExternalAuthorDto dto, CancellationToken ct)
    {
        // ── By ORCID ──────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(dto.AuthorId))
        {
            var byOrcid = await _dbContext.Authors
                .FirstOrDefaultAsync(a => a.Orcid == dto.AuthorId, ct);
            if (byOrcid is not null) return byOrcid;

            // Check change tracker — avoids duplicate key if same author
            // appears in multiple papers processed in this batch
            var localByOrcid = _dbContext.Authors.Local
                .FirstOrDefault(a => a.Orcid == dto.AuthorId);
            if (localByOrcid is not null) return localByOrcid;
        }

        // ── By full name (fallback) ───────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var nameLower = dto.Name.ToLower();

            var byName = await _dbContext.Authors
                .FirstOrDefaultAsync(a => a.FullName.ToLower() == nameLower, ct);
            if (byName is not null) return byName;

            var localByName = _dbContext.Authors.Local
                .FirstOrDefault(a => a.FullName.ToLower() == nameLower);
            if (localByName is not null) return localByName;
        }

        // ── Create new ────────────────────────────────────────────────────────
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FullName = dto.Name,
            Orcid = dto.AuthorId,
            PublicationCount = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Authors.Add(author);
        return author;
    }

    private static readonly Dictionary<ResearchDomain, string[]> Signals = new()
    {
        [ResearchDomain.ComputerScience] =
        [
            "machine learning", "neural network", "deep learning", "algorithm",
            "artificial intelligence", "nlp", "computer vision", "data mining",
            "software", "computing", "cybersecurity"
        ],

        [ResearchDomain.ArtificialIntelligence] =
        [
            "artificial intelligence", "machine learning", "deep learning",
            "neural network", "computer vision", "natural language processing",
            "reinforcement learning", "transformer", "llm", "generative ai"
        ]
    };

    private static ResearchDomain InferDomain(string[]? keywords, string? title)
    {
        var text = string.Join(" ", (keywords ?? []).Append(title ?? string.Empty))
            .ToLowerInvariant();

        return Signals
            .OrderByDescending(kv => kv.Value.Count(text.Contains))
            .First()
            .Key;
    }
}
