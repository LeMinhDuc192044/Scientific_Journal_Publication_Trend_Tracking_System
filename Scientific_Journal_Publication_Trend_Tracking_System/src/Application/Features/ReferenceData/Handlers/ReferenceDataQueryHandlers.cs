using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.Handlers;

public sealed class GetJournalsQueryHandler
    : IRequestHandler<GetJournalsQuery, PagedResult<JournalListItemDto>>
{
    private readonly AppDbContext _dbContext;

    public GetJournalsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<JournalListItemDto>> Handle(
        GetJournalsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Journals.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(j => j.Title.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(j => j.Title)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(j => new JournalListItemDto
            {
                Id = j.Id,
                Title = j.Title,
                Issn = j.Issn,
                Publisher = j.Publisher,
                TotalPapersPublished = j.TotalPapersPublished
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<JournalListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

public sealed class GetResearchTopicsQueryHandler
    : IRequestHandler<GetResearchTopicsQuery, PagedResult<ResearchTopicListItemDto>>
{
    private readonly AppDbContext _dbContext;

    public GetResearchTopicsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ResearchTopicListItemDto>> Handle(
        GetResearchTopicsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.ResearchTopics.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new ResearchTopicListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Domain = t.Domain,
                PapersCount = t.PapersCount
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ResearchTopicListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
