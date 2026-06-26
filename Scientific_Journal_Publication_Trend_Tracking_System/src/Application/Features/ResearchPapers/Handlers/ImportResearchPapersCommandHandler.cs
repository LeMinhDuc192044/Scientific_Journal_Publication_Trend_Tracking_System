using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchPapers.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.ExternalApis.ExternalPaperDto;
using System.Threading;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchPapers.Handlers
{
    public class ImportResearchPapersCommandHandler : IRequestHandler<ImportResearchPapersCommand, ImportPapersResult>  
    {
        private readonly IExternalApiSyncService _api;
        private readonly AppDbContext _db;
        private readonly ILogger<ImportResearchPapersCommandHandler> _logger;

        public ImportResearchPapersCommandHandler(
            IExternalApiSyncService api,
            AppDbContext db,
            ILogger<ImportResearchPapersCommandHandler> logger)
        {
            _api = api;
            _db = db;
            _logger = logger;
        }

        //-- --------------------------------------------------------------------------
        //private async Task<ResearchPaperDto> AddResearchPaperByLink(ImportResearchPapersByLinksCommand dto, CancellationToken cancellationToken)
        //{
        //    IReadOnlyList<ExternalPaperDto> papers;
            
        //}

        public async Task<ImportPapersResult> Handle(
            ImportResearchPapersCommand request,
            CancellationToken cancellationToken)
        {
            var errors = new List<string>();
            var imported = 0;
            var skipped = 0;

            // ── 1. Fetch from external API ────────────────────────────────────────
            _logger.LogInformation(
                "Importing papers — query: '{Query}', source: {Source}, limit: {Limit}",
                request.Query, request.ApiSource, request.Limit);

            IReadOnlyList<ExternalPaperDto> papers;
            try
            {
                papers = request.ApiSource.ToLowerInvariant() switch
                {
                    "semanticscholar" => await _api.SearchSemanticScholarAsync(
                                             request.Query, request.Limit, cancellationToken),
                    "openalex" => await _api.SearchOpenAlexAsync(
                                             request.Query, request.Limit, cancellationToken),
                    "crossref" => await _api.SearchCrossrefAsync(
                                             request.Query, request.Limit, cancellationToken),
                    _ => throw new ArgumentException(
                             $"Unknown source '{request.ApiSource}'. " +
                             "Valid values: semanticscholar, openalex, crossref")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch from external API");
                return new ImportPapersResult(0, 0, [$"External API error: {ex.Message}"]);
            }

            _logger.LogInformation("Fetched {Count} papers from {Source}",
                papers.Count, request.ApiSource);

            // ── 2. Insert each paper ──────────────────────────────────────────────
            foreach (var dto in papers)
            {
                try
                {
                    // Skip if already imported (e.g. duplicate query was run before)
                    var alreadyExists = await _db.ResearchPapers.AnyAsync(
                        p => p.ExternalId == dto.PaperId && p.ApiSource == request.ApiSource,
                        cancellationToken);

                    if (alreadyExists)
                    {
                        skipped++;
                        continue;
                    }

                    var paper = await BuildPaperAsync(dto, request.ApiSource, cancellationToken);
                    _db.ResearchPapers.Add(paper);
                    imported++;
                }
                catch (Exception ex)
                {
                    var msg = $"Failed to import '{dto.Title}': {ex.Message}";
                    _logger.LogWarning(ex, msg);
                    errors.Add(msg);
                }
            }

            // ── 3. Save all at once ───────────────────────────────────────────────
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Import done — imported: {I}, skipped: {S}, errors: {E}",
                imported, skipped, errors.Count);

            return new ImportPapersResult(imported, skipped, errors);
        }

        // ── Build a new ResearchPaper from the external DTO ───────────────────────

        private async Task<ResearchPaper> BuildPaperAsync(
            ExternalPaperDto dto,
            string apiSource,
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
                IsFullTextAvailable = false,          // full-text never imported
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Journal — reuse existing or create new
            if (!string.IsNullOrWhiteSpace(dto.JournalName))
                paper.Journal = await GetOrAddJournalAsync(dto.JournalName, ct);

            // Authors — reuse existing (by ORCID or name) or create new
            if (dto.Authors is { Count: > 0 })
                foreach (var a in dto.Authors)
                    paper.Authors.Add(await GetOrAddAuthorAsync(a, ct));

            return paper;
        }

        // ── Journal: reuse by title, create if missing ────────────────────────────

        private async Task<Journal> GetOrAddJournalAsync(string name, CancellationToken ct)
        {
            var normalised = name.Trim().ToLower();

            // 1. Check DB (already saved records)
            var journal = await _db.Journals
                .FirstOrDefaultAsync(j => j.Title.ToLower() == normalised, ct);
            if (journal is not null) return journal;

            // 2. Check change tracker — catches journals added earlier in this same
            //    batch before SaveChangesAsync has been called
            journal = _db.Journals.Local
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
            _db.Journals.Add(journal);
            return journal;
        }

        // ── Author: reuse by ORCID → full name, create if missing ────────────────

        private async Task<Author> GetOrAddAuthorAsync(ExternalAuthorDto dto, CancellationToken ct)
        {
            // ── By ORCID (most reliable) ──────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(dto.AuthorId))
            {
                // 1. Check DB
                var byOrcid = await _db.Authors
                    .FirstOrDefaultAsync(a => a.Orcid == dto.AuthorId, ct);
                if (byOrcid is not null) return byOrcid;

                // 2. Check change tracker — same author appearing in multiple papers
                //    in this batch won't be in the DB yet, but IS in Local
                var localByOrcid = _db.Authors.Local
                    .FirstOrDefault(a => a.Orcid == dto.AuthorId);
                if (localByOrcid is not null) return localByOrcid;
            }

            // ── By full name (fallback) ───────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var nameLower = dto.Name.ToLower();

                var byName = await _db.Authors
                    .FirstOrDefaultAsync(a => a.FullName.ToLower() == nameLower, ct);
                if (byName is not null) return byName;

                var localByName = _db.Authors.Local
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
            _db.Authors.Add(author);
            return author;
        }

        




        // ── Domain inference: score keywords against known signals ────────────────

        private static readonly Dictionary<ResearchDomain, string[]> Signals = new()
        {
            [ResearchDomain.ComputerScience] =
                ["machine learning", "neural network", "deep learning", "algorithm",
             "artificial intelligence", "nlp", "computer vision", "data mining",
             "software", "computing", "cybersecurity"],

            [ResearchDomain.ArtificialIntelligence] =
                ["artificial intelligence", "machine learning", "deep learning",
            "neural network", "computer vision","natural language processing", "reinforcement learning",
            "transformer", "llm", "generative ai"]


        };

        private static ResearchDomain InferDomain(string[]? keywords, string? title)
        {
            var text = string.Join(" ", (keywords ?? []).Append(title ?? ""))
                            .ToLowerInvariant();

            return Signals
                .OrderByDescending(kv => kv.Value.Count(s => text.Contains(s)))
                .First().Key;
        }
    }
}
