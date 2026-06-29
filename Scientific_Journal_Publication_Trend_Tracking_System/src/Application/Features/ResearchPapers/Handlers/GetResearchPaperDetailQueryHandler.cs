using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Handlers;

/// <summary>
/// Handler for getting research paper details
/// </summary>
public class GetResearchPaperDetailQueryHandler : IRequestHandler<GetResearchPaperDetailQuery, ResearchPaperDto?>
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetResearchPaperDetailQueryHandler(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ResearchPaperDto?> Handle(GetResearchPaperDetailQuery request, CancellationToken cancellationToken)
    {
        var paper = await _dbContext.ResearchPapers
            .AsNoTracking()
            .Include(p => p.Journal)
            .Include(p => p.Authors)
            .Include(p => p.ResearchTopics)
            .FirstOrDefaultAsync(p => p.Id == request.PaperId, cancellationToken);

        if (paper == null)
            return null;

        return _mapper.Map<ResearchPaperDto>(paper);
    }
}
