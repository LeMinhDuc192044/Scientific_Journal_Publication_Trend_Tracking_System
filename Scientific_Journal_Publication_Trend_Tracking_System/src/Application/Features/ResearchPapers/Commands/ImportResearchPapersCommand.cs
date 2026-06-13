using MediatR;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchPapers.Commands;

public record ImportResearchPapersCommand(
        string Query,       // e.g. "transformer language model"
        string ApiSource,   // "semanticscholar" | "openalex" | "crossref"
        int Limit = 20   // number of papers to fetch
    ) : IRequest<ImportPapersResult>;

public record ImportPapersResult(
    int Imported,
    int Skipped,       // already existed (ExternalId match)
    IReadOnlyList<string> Errors
);


