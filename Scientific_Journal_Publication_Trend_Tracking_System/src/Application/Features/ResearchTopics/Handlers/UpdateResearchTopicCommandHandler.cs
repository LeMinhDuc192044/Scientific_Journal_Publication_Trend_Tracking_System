using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.DTO;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Handlers;

public class UpdateResearchTopicCommandHandler : IRequestHandler<UpdateResearchTopicCommand, ResearchTopicDto>
{
    private readonly AppDbContext _context;

    public UpdateResearchTopicCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ResearchTopicDto> Handle(UpdateResearchTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _context.ResearchTopics
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (topic == null)
            throw new NotFoundException($"Research topic with ID {request.Id} not found");

        var duplicateName = await _context.ResearchTopics
            .AnyAsync(t => t.Name == request.Name && t.Id != request.Id, cancellationToken);

        if (duplicateName)
            throw new ConflictException($"A research topic named '{request.Name}' already exists");

        if(topic.Name != null) 
            topic.Name = request.Name;

        if(topic.Description != null)
            topic.Description = request.Description;

        
        topic.Domain = request.Domain;
        // UpdatedAt: omit if your audit interceptor handles it automatically

        await _context.SaveChangesAsync(cancellationToken);

        return CreateResearchTopicCommandHandler.ToDto(topic);
    }
}
