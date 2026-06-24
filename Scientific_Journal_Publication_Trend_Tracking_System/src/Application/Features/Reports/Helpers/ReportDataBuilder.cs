using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Helpers;

internal static class ReportDataBuilder
{
  private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

  public static async Task<AnalyticalReportDto> BuildAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
  {
    var totalPapers = await unitOfWork.ResearchPapers.CountAsync(cancellationToken: cancellationToken);
    var activeUsers = await unitOfWork.Users.CountAsync(u => u.IsActive, cancellationToken);
    var totalBookmarks = await unitOfWork.Bookmarks.CountAsync(cancellationToken: cancellationToken);

    var topDomains = await unitOfWork.ResearchPapers.GetQueryable()
        .GroupBy(p => p.Domain)
        .Select(g => new DomainStatDto
        {
          Domain = g.Key.ToString(),
          PaperCount = g.Count()
        })
        .OrderByDescending(d => d.PaperCount)
        .Take(5)
        .ToListAsync(cancellationToken);

    var mostCited = await unitOfWork.ResearchPapers.GetQueryable()
        .OrderByDescending(p => p.CitationCount)
        .Take(10)
        .Select(p => new CitedPaperDto
        {
          Id = p.Id,
          Title = p.Title,
          CitationCount = p.CitationCount,
          PublicationYear = p.PublicationYear
        })
        .ToListAsync(cancellationToken);

    var byYear = await unitOfWork.ResearchPapers.GetQueryable()
        .GroupBy(p => p.PublicationYear)
        .Select(g => new YearlyPublicationDto
        {
          Year = g.Key,
          Count = g.Count()
        })
        .OrderBy(y => y.Year)
        .ToListAsync(cancellationToken);

    var topKeywords = await unitOfWork.Keywords.GetQueryable()
        .OrderByDescending(k => k.FrequencyCount)
        .Take(10)
        .Select(k => new KeywordStatDto
        {
          Name = k.Name,
          Frequency = k.FrequencyCount
        })
        .ToListAsync(cancellationToken);

    return new AnalyticalReportDto
    {
      Id = Guid.Empty,
      Title = "Publication Trend Summary",
      Description = "Live analytical summary of publication trends",
      TotalPapersCount = totalPapers,
      ActiveUsersCount = activeUsers,
      TotalBookmarksCount = totalBookmarks,
      TopResearchDomains = topDomains,
      MostCitedPapers = mostCited,
      PublicationsByYear = byYear,
      TopKeywords = topKeywords,
      GeneratedAt = DateTime.UtcNow
    };
  }

  public static DashboardReport ToEntity(AnalyticalReportDto report, string? customTitle)
  {
    var now = DateTime.UtcNow;
    var extraData = new
    {
      report.TotalBookmarksCount,
      report.PublicationsByYear,
      report.TopKeywords
    };

    return new DashboardReport
    {
      Id = Guid.NewGuid(),
      Title = string.IsNullOrWhiteSpace(customTitle) ? report.Title : customTitle.Trim(),
      Description = JsonSerializer.Serialize(extraData, JsonOptions),
      TotalPapersCount = report.TotalPapersCount,
      ActiveUsersCount = report.ActiveUsersCount,
      TopResearchDomains = JsonSerializer.Serialize(report.TopResearchDomains, JsonOptions),
      MostCitedPapers = JsonSerializer.Serialize(report.MostCitedPapers, JsonOptions),
      GeneratedAt = now,
      CreatedAt = now,
      UpdatedAt = now
    };
  }

  public static AnalyticalReportDto FromEntity(DashboardReport entity)
  {
    var topDomains = Deserialize<List<DomainStatDto>>(entity.TopResearchDomains) ?? new();
    var mostCited = Deserialize<List<CitedPaperDto>>(entity.MostCitedPapers) ?? new();

    var totalBookmarks = 0;
    var byYear = new List<YearlyPublicationDto>();
    var topKeywords = new List<KeywordStatDto>();

    if (!string.IsNullOrWhiteSpace(entity.Description))
    {
      using var doc = JsonDocument.Parse(entity.Description);
      var root = doc.RootElement;

      if (root.TryGetProperty("totalBookmarksCount", out var bookmarks))
        totalBookmarks = bookmarks.GetInt32();

      if (root.TryGetProperty("publicationsByYear", out var years))
        byYear = JsonSerializer.Deserialize<List<YearlyPublicationDto>>(years.GetRawText(), JsonOptions) ?? new();

      if (root.TryGetProperty("topKeywords", out var keywords))
        topKeywords = JsonSerializer.Deserialize<List<KeywordStatDto>>(keywords.GetRawText(), JsonOptions) ?? new();
    }

    return new AnalyticalReportDto
    {
      Id = entity.Id,
      Title = entity.Title,
      Description = "Saved analytical report",
      TotalPapersCount = entity.TotalPapersCount,
      ActiveUsersCount = entity.ActiveUsersCount,
      TotalBookmarksCount = totalBookmarks,
      TopResearchDomains = topDomains,
      MostCitedPapers = mostCited,
      PublicationsByYear = byYear,
      TopKeywords = topKeywords,
      GeneratedAt = entity.GeneratedAt
    };
  }

  private static T? Deserialize<T>(string json)
  {
    if (string.IsNullOrWhiteSpace(json))
      return default;

    return JsonSerializer.Deserialize<T>(json, JsonOptions);
  }
}
