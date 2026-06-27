using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchPapers.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.ExternalApis.ExternalPaperDto;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchPapers.Handlers;


public class ImportSingleResearchPaperByLinkCommandHandler : IRequestHandler<ImportResearchPaperByLinkCommand, ImportSinglePaperResult>
{
    private readonly IExternalApiSyncService _api;
    private readonly AppDbContext _db;
    private readonly ILogger<ImportResearchPaperByLinkCommand> _logger;

    public ImportSingleResearchPaperByLinkCommandHandler(
        IExternalApiSyncService api,
        AppDbContext db,
        ILogger<ImportResearchPaperByLinkCommand> logger)
    {
        _api = api;
        _db = db;
        _logger = logger;
    }

    public async Task<ImportSinglePaperResult> Handle(
        ImportResearchPaperByLinkCommand request,
        CancellationToken cancellationToken)
    {
        var apiSource = request.ApiSource.ToLowerInvariant();

        if (apiSource is not ("semanticscholar" or "openalex" or "crossref"))
            throw new ArgumentException(
                $"Unknown source '{request.ApiSource}'. Valid values: semanticscholar, openalex, crossref");

        // ── 1. Fetch the single paper from the external API ────────────────────
        _logger.LogInformation("Importing single paper — link: '{Link}', source: {Source}",
            request.Link, apiSource);

        ExternalPaperDto? dto;
        try
        {
            dto = await _api.GetByLinkAsync(request.Link, apiSource, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new NotFoundException(
                $"Could not fetch paper from {apiSource} for link '{request.Link}': {ex.Message}");
        }

        if (dto is null)
            throw new NotFoundException($"No paper found at '{request.Link}' on {apiSource}");

        // ── 2. Reject duplicates (already imported) ─────────────────────────────
        var alreadyExists = await _db.ResearchPapers.AnyAsync(
            p => p.ExternalId == dto.PaperId && p.ApiSource == apiSource, cancellationToken);

        if (alreadyExists)
            throw new ConflictException(
                $"This paper has already been imported (ExternalId: {dto.PaperId}, source: {apiSource})");

        // ── 3. Validate the chosen ResearchTopics exist ─────────────────────────
        var requestedTopicIds = request.ResearchTopicIds.Distinct().ToList();
        var topics = new List<ResearchTopic>();

        if (requestedTopicIds.Count > 0)
        {
            topics = await _db.ResearchTopics
                .Where(t => requestedTopicIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            var missingIds = requestedTopicIds.Except(topics.Select(t => t.Id)).ToList();
            if (missingIds.Count > 0)
                throw new NotFoundException(
                    $"Research topic(s) not found: {string.Join(", ", missingIds)}");
        }

        // ── 4. Build the paper entity ────────────────────────────────────────────
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
            Domain = DetermineDomain(dto.Keywords, dto.Title, topics),
            IsFullTextAvailable = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(dto.JournalName))
            paper.Journal = await GetOrAddJournalAsync(dto.JournalName, cancellationToken);

        if (dto.Authors is { Count: > 0 })
            foreach (var a in dto.Authors)
                paper.Authors.Add(await GetOrAddAuthorAsync(a, cancellationToken));

        // ── 5. Link to chosen topics + bump PapersCount on each ────────────────
        foreach (var topic in topics)
        {
            paper.ResearchTopics.Add(topic);   // many-to-many join row created automatically
            topic.PapersCount += 1;
            topic.UpdatedAt = DateTime.UtcNow;
        }

        _db.ResearchPapers.Add(paper);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Imported paper '{Title}' and linked it to {Count} topic(s)", paper.Title, topics.Count);

        return new ImportSinglePaperResult(
            PaperId: paper.Id,
            Title: paper.Title,
            CitationCount: paper.CitationCount,
            JournalName: paper.Journal?.Title,
            AuthorNames: paper.Authors.Select(a => a.FullName).ToList(),
            LinkedTopicNames: topics.Select(t => t.Name).ToList()
        );
    }

    // ── Journal: reuse by title, create if missing ────────────────────────────

    private async Task<Journal> GetOrAddJournalAsync(string name, CancellationToken ct)
    {
        var normalised = name.Trim().ToLower();

        var journal = await _db.Journals
            .FirstOrDefaultAsync(j => j.Title.ToLower() == normalised, ct);
        if (journal is not null) return journal;

        journal = _db.Journals.Local
            .FirstOrDefault(j => j.Title.ToLower() == normalised);
        if (journal is not null) return journal;

        journal = new Journal
        {
            Id = Guid.NewGuid(),
            Title = name.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Journals.Add(journal);
        return journal;
    }

    // ── Author: reuse by ORCID → full name, create if missing ────────────────

    private async Task<Author> GetOrAddAuthorAsync(ExternalAuthorDto dto, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(dto.AuthorId))
        {
            var byOrcid = await _db.Authors.FirstOrDefaultAsync(a => a.Orcid == dto.AuthorId, ct);
            if (byOrcid is not null) return byOrcid;

            var localByOrcid = _db.Authors.Local.FirstOrDefault(a => a.Orcid == dto.AuthorId);
            if (localByOrcid is not null) return localByOrcid;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var nameLower = dto.Name.ToLower();

            var byName = await _db.Authors.FirstOrDefaultAsync(a => a.FullName.ToLower() == nameLower, ct);
            if (byName is not null) return byName;

            var localByName = _db.Authors.Local.FirstOrDefault(a => a.FullName.ToLower() == nameLower);
            if (localByName is not null) return localByName;
        }

        var author = new Author
        {
            Id = Guid.NewGuid(),
            FullName = dto.Name,
            Orcid = dto.AuthorId,
            PublicationCount = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Authors.Add(author);
        return author;
    }

    // ── Domain: prefer the chosen topics' domain, fall back to keyword heuristic ──

    private static readonly Dictionary<ResearchDomain, string[]> Signals = new()
    {
        [ResearchDomain.ComputerScience] =
            ["software", "programming", "computer system",
            "operating system", "distributed system", "database",
            "network", "cybersecurity", "cloud computing",
            "compiler", "algorithm", "data structure"],
        [ResearchDomain.ArtificialIntelligence] =
            ["artificial intelligence", "machine learning", "deep learning",
            "neural network", "transformer", "large language model", "llm",
            "computer vision", "natural language processing", "nlp",
            "reinforcement learning", "generative ai", "diffusion model",
            "recommendation system", "knowledge graph", "data mining"]
    };

    private static ResearchDomain DetermineDomain(
        string[]? keywords, string? title, List<ResearchTopic> linkedTopics)
    {
        // Prefer the domain shared by the chosen topics — more accurate than guessing
        // from keywords, since the user has already categorized this paper by hand.
        if (linkedTopics.Count > 0)
        {
            return linkedTopics
                .GroupBy(t => t.Domain)
                .OrderByDescending(g => g.Count())
                .First().Key;
        }

        var text = string.Join(" ", (keywords ?? []).Append(title ?? "")).ToLowerInvariant();

        return Signals
            .OrderByDescending(kv => kv.Value.Count(s => text.Contains(s)))
            .First().Key;
    }
}

