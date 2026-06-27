using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.DTO;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Handlers;

public class SearchResearchTopicsByNameQueryHandler
    : IRequestHandler<SearchResearchTopicsByNameQuery, PaginatedList<ResearchTopicDto>>
{
    private readonly AppDbContext _context;

    public SearchResearchTopicsByNameQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ResearchTopicDto>> Handle(
        SearchResearchTopicsByNameQuery request,
        CancellationToken cancellationToken)
    {
        // EF Core translates .Contains() into SQL: WHERE Name LIKE '%value%'
        var query = _context.ResearchTopics
            .AsNoTracking()
            .Where(t => t.Name.Contains(request.Name));

        var totalCount = await query.CountAsync(cancellationToken);

        var topics = await query
            .OrderBy(t => t.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<ResearchTopicDto>
        {
            Items = topics.Select(CreateResearchTopicCommandHandler.ToDto).ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
