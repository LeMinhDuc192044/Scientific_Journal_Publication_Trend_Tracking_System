using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.DTO;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Handlers;


public class GetResearchTopicByIdQueryHandler : IRequestHandler<GetResearchTopicByIdQuery, ResearchTopicDto>
{
    private readonly AppDbContext _context;

    public GetResearchTopicByIdQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ResearchTopicDto> Handle(GetResearchTopicByIdQuery request, CancellationToken cancellationToken)
    {
        var topic = await _context.ResearchTopics
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (topic == null)
            throw new NotFoundException($"Research topic with ID {request.Id} not found");

        return CreateResearchTopicCommandHandler.ToDto(topic);
    }
}

