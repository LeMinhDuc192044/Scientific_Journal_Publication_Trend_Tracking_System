namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis.Crossref;

/// <summary>
/// DTOs for Crossref API responses
/// </summary>
public record CrossrefResponse(
    CrossrefMessage? Message);

public record CrossrefMessage(
    CrossrefItem[]? Items,
    int? TotalResults);

public record CrossrefItem(
    string? Title,
    string? Abstract,
    int? Published,
    string[]? Keywords,
    CrossrefAuthor[]? Author,
    string? Doi,
    int? IsReferenced,
    CrossrefContainer? Container);

public record CrossrefAuthor(
    string? Given,
    string? Family);

public record CrossrefContainer(
    string? Title);
