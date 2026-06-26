using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.DTO;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Handlers;

public class CreateResearchTopicCommandHandler : IRequestHandler<CreateResearchTopicCommand, ResearchTopicDto>
{
    private readonly AppDbContext _context;

    public CreateResearchTopicCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ResearchTopicDto> Handle(CreateResearchTopicCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _context.ResearchTopics
            .AnyAsync(t => t.Name == request.Name, cancellationToken);

        if (nameExists)
            throw new ConflictException($"A research topic named '{request.Name}' already exists");

        var topic = new ResearchTopic
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Domain = request.Domain,
            PapersCount = 0
            // CreatedAt / UpdatedAt: omit if you have an audit SaveChanges interceptor
            // setting these automatically via IAuditableEntity. Otherwise set DateTime.UtcNow here.
        };

        _context.ResearchTopics.Add(topic);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(topic);
    }

    public static ResearchTopicDto ToDto(ResearchTopic topic) => new()
    {
        Id = topic.Id,
        Name = topic.Name,
        Description = topic.Description,
        Domain = topic.Domain,
        PapersCount = topic.PapersCount,
        CreatedAt = topic.CreatedAt,
        UpdatedAt = topic.UpdatedAt
    };
}