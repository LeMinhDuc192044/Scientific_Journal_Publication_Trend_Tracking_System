using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.DTO;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Handlers;

public class GetResearchTopicsListHandler : IRequestHandler<GetResearchTopicsListCommand, List<ResearchTopicDto>>
{
    private readonly AppDbContext _context;

    public GetResearchTopicsListHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ResearchTopicDto>> Handle(GetResearchTopicsListCommand request, CancellationToken cancellationToken)
    {
        var topics = await _context.ResearchTopics
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return topics.Select(CreateResearchTopicCommandHandler.ToDto).ToList();
    }
}

