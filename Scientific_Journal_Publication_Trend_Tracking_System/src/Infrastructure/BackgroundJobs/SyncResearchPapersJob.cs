using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Commands;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.BackgroundJobs
{
    public class SyncResearchPapersJob
    {
        private readonly IMediator _mediator;

        public SyncResearchPapersJob(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Execute()
        {
            await _mediator.Send(new SyncResearchPapersCommand(
                "Artificial Intelligence",
                "openalex",
                100
            ));
        }
    }
}
