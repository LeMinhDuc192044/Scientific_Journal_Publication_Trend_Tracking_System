using AutoMapper;
using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Handlers;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;


namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Handlers;

/// <summary>
/// Handler for getting paper details
/// </summary>
public class GetPaperDetailsQueryHandler : IRequestHandler<GetPaperDetailsQuery, ResearchPaperDto>
{
    private readonly IResearchPaperRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;
    public GetPaperDetailsQueryHandler(
        IResearchPaperRepository repository,
        IMapper mapper,
        ILogger <LoginCommandHandler>logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResearchPaperDto> Handle(GetPaperDetailsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting paper details for ID: {PaperId}", request.PaperId);

        var paper = await _repository.GetByIdAsync(request.PaperId, cancellationToken);
        if (paper == null)
        {
            _logger.LogWarning("Paper not found: {PaperId}", request.PaperId);
            throw new NotFoundException($"Research paper with ID {request.PaperId} not found");
        }

        return _mapper.Map<ResearchPaperDto>(paper);
    }
}
