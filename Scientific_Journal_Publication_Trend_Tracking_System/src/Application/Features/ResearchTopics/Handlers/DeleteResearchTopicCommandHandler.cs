using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Handlers;

public class DeleteResearchTopicCommandHandler : IRequestHandler<DeleteResearchTopicCommand>
{
    private readonly AppDbContext _context;

    public DeleteResearchTopicCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteResearchTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _context.ResearchTopics
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (topic == null)
            throw new NotFoundException($"Research topic with ID {request.Id} not found");

        _context.ResearchTopics.Remove(topic);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
