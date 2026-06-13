using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis.SemanticScholar;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.ExternalApis.ExternalPaperDto;
public record ExternalPaperDto(
    string PaperId,        // unique ID from the external API
    string Title,
    string? Abstract,
    string[]? Keywords,
    int? Year,
    string? Url,
    int? CitationCount,
    string? Doi,
    string? JournalName,
    IReadOnlyList<ExternalAuthorDto>? Authors
);

public record ExternalAuthorDto(
    string Name,
    string? AuthorId       // ORCID / OpenAlex ID / SemanticScholar AuthorId
);

public record ExternalPaperSearchResponse(
    ExternalPaperDto[]? Data,
    int? Total);
