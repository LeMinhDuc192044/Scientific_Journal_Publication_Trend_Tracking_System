using MediatR;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;

public record DeleteResearchTopicCommand(Guid Id) : IRequest;
