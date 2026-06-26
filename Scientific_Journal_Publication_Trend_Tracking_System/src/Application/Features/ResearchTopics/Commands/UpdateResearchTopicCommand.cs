using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.DTO;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;
public record UpdateResearchTopicCommand(
    Guid Id,
    string Name,
    string Description,
    ResearchDomain Domain) : IRequest<ResearchTopicDto>;