using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for ResearchPaper entity with specific queries
/// </summary>
public interface IResearchPaperRepository : IRepository<ResearchPaper>
{
    Task<ResearchPaper?> GetByExternalIdAsync(string externalId, string apiSource, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResearchPaper>> SearchAsync(string? keyword, string? author, string? journal, int? fromYear, int? toYear, ResearchDomain? domain, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<int> GetSearchCountAsync(string? keyword, string? author, string? journal, int? fromYear, int? toYear, ResearchDomain? domain, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResearchPaper>> GetPopularPapersAsync(int topCount = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResearchPaper>> GetPapersByTopicAsync(Guid topicId, int pageSize = 20, CancellationToken cancellationToken = default);
}

public class ResearchPaperRepository : Repository<ResearchPaper>, IResearchPaperRepository
{
    public ResearchPaperRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ResearchPaper?> GetByExternalIdAsync(string externalId, string apiSource, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExternalId == externalId && p.ApiSource == apiSource, cancellationToken);
    }

    public async Task<IEnumerable<ResearchPaper>> SearchAsync(string? keyword, string? author, string? journal, int? fromYear, int? toYear, ResearchDomain? domain, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => 
                p.Title.Contains(keyword) || 
                p.Abstract!.Contains(keyword) ||
                p.Keywords.Any(k => k.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(p => p.Authors.Any(a => a.FullName.Contains(author)));
        }

        if (!string.IsNullOrWhiteSpace(journal))
        {
            query = query.Where(p => p.Journal != null && p.Journal.Title.Contains(journal));
        }

        if (fromYear.HasValue)
        {
            query = query.Where(p => p.PublicationYear >= fromYear.Value);
        }

        if (toYear.HasValue)
        {
            query = query.Where(p => p.PublicationYear <= toYear.Value);
        }

        if (domain.HasValue)
        {
            query = query.Where(p => p.Domain == domain.Value);
        }

        return await query
            .OrderByDescending(p => p.PublicationYear)
            .ThenByDescending(p => p.CitationCount)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Journal)
            .Include(p => p.Authors)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetSearchCountAsync(string? keyword, string? author, string? journal, int? fromYear, int? toYear, ResearchDomain? domain, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => 
                p.Title.Contains(keyword) || 
                p.Abstract!.Contains(keyword) ||
                p.Keywords.Any(k => k.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(p => p.Authors.Any(a => a.FullName.Contains(author)));
        }

        if (!string.IsNullOrWhiteSpace(journal))
        {
            query = query.Where(p => p.Journal != null && p.Journal.Title.Contains(journal));
        }

        if (fromYear.HasValue)
        {
            query = query.Where(p => p.PublicationYear >= fromYear.Value);
        }

        if (toYear.HasValue)
        {
            query = query.Where(p => p.PublicationYear <= toYear.Value);
        }

        if (domain.HasValue)
        {
            query = query.Where(p => p.Domain == domain.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<ResearchPaper>> GetPopularPapersAsync(int topCount = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderByDescending(p => p.CitationCount)
            .Take(topCount)
            .Include(p => p.Journal)
            .Include(p => p.Authors)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ResearchPaper>> GetPapersByTopicAsync(Guid topicId, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.ResearchTopics.Any(t => t.Id == topicId))
            .OrderByDescending(p => p.PublicationYear)
            .Take(pageSize)
            .Include(p => p.Journal)
            .Include(p => p.Authors)
            .ToListAsync(cancellationToken);
    }
}
