using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;
using Scientific_Journal_Publication_Trend_Tracking_System.Tests.Helpers;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Tests.Trends;

public class TrendAnalyticsRepositoryTests : IAsyncLifetime
{
  private readonly AppDbContext _context;
  private readonly TrendAnalyticsRepository _repository;

  public TrendAnalyticsRepositoryTests()
  {
    _context = TestDbContextFactory.Create();
    _repository = new TrendAnalyticsRepository(_context);
  }

  public async Task InitializeAsync() => await TrendsTestDataSeeder.SeedAsync(_context);

  public Task DisposeAsync()
  {
    _context.Dispose();
    return Task.CompletedTask;
  }

  [Fact]
  public async Task GetKeywordTrendAsync_MatchesKeywordInTitleAndKeywords()
  {
    var result = await _repository.GetKeywordTrendAsync(
        "machine learning", 2020, 2024);

    Assert.Equal("machine learning", result.Keyword);
    Assert.Equal(4, result.TotalPublications);
    Assert.Equal(4, result.DataPoints.Count); // 2020, 2022, 2023, 2024
    Assert.Contains(result.DataPoints, p => p.Year == 2023 && p.PublicationCount == 1);
  }

  [Fact]
  public async Task GetKeywordTrendAsync_NoMatches_ReturnsEmptyDataPoints()
  {
    var result = await _repository.GetKeywordTrendAsync(
        "quantum computing", 2020, 2024);

    Assert.Equal(0, result.TotalPublications);
    Assert.Empty(result.DataPoints);
    Assert.Equal(0, result.OverallGrowthRate);
  }

  [Fact]
  public async Task GetKeywordTrendAsync_CalculatesGrowthRateBetweenYears()
  {
    var result = await _repository.GetKeywordTrendAsync(
        "machine learning", 2020, 2024);

    var point2023 = result.DataPoints.First(p => p.Year == 2023);
    var point2024 = result.DataPoints.First(p => p.Year == 2024);

    Assert.Equal(0, point2023.GrowthRate); // first data point in range
    Assert.Equal(0, point2024.GrowthRate); // same count as previous (1 vs 1)
  }

  [Fact]
  public async Task GetKeywordTrendAsync_SwapsInvertedYearRange()
  {
    var result = await _repository.GetKeywordTrendAsync(
        "machine learning", 2024, 2020);

    Assert.True(result.FromYear <= result.ToYear);
    Assert.Equal(2020, result.FromYear);
    Assert.Equal(2024, result.ToYear);
  }

  [Fact]
  public async Task GetTopicTrendAsync_UnknownTopic_ReturnsNull()
  {
    var result = await _repository.GetTopicTrendAsync(
        Guid.Parse("99999999-9999-9999-9999-999999999999"), 2020, 2024);

    Assert.Null(result);
  }

  [Fact]
  public async Task GetTopicTrendAsync_ExistingTopic_ReturnsTrendData()
  {
    var result = await _repository.GetTopicTrendAsync(
        TrendsTestDataSeeder.MachineLearningTopicId, 2020, 2024);

    Assert.NotNull(result);
    Assert.Equal("Machine Learning", result!.TopicName);
    Assert.True(result.TotalPublications >= 2);
    Assert.NotEmpty(result.DataPoints);
  }

  [Fact]
  public async Task GetTrendingTopicsAsync_ReturnsRankedItemsUsingLatestDataYear()
  {
    var result = await _repository.GetTrendingTopicsAsync(5, 2);

    // Seed max year is 2024 → recent period 2023-2024, previous 2021-2022
    Assert.Equal(2023, result.RecentPeriodStart);
    Assert.Equal(2024, result.RecentPeriodEnd);
    Assert.Equal(2021, result.PreviousPeriodStart);
    Assert.Equal(2022, result.PreviousPeriodEnd);
    Assert.NotEmpty(result.Items);
    Assert.All(result.Items, item => Assert.True(item.Rank > 0));
    Assert.Equal(1, result.Items[0].Rank);
  }

  [Fact]
  public async Task GetTrendingTopicsAsync_RespectsTopCount()
  {
    var result = await _repository.GetTrendingTopicsAsync(2, 2);

    Assert.True(result.Items.Count <= 2);
  }

  [Fact]
  public async Task GetDashboardSummaryAsync_ReturnsCorrectCounts()
  {
    var result = await _repository.GetDashboardSummaryAsync();

    Assert.Equal(6, result.TotalPapers);
    Assert.Equal(2, result.TotalJournals);
    Assert.Equal(1, result.TotalAuthors);
    Assert.Equal(1, result.TotalResearchTopics);
    Assert.True(result.AverageCitations > 0);
  }

  [Fact]
  public async Task GetPublicationsByYearChartAsync_ReturnsSortedYearLabels()
  {
    var result = await _repository.GetPublicationsByYearChartAsync(2020, 2024);

    Assert.Equal("line", result.ChartType);
    Assert.NotEmpty(result.Labels);
    Assert.Equal(result.Labels.Count, result.Values.Count);
    Assert.Equal(result.Labels, result.Labels.OrderBy(l => int.Parse(l)).ToList());
  }

  [Fact]
  public async Task GetTopKeywordsChartAsync_ReturnsMostFrequentKeywords()
  {
    var result = await _repository.GetTopKeywordsChartAsync(3);

    Assert.Equal("bar", result.ChartType);
    Assert.True(result.Labels.Count <= 3);
    Assert.Equal("machine learning", result.Labels[0]);
  }

  [Fact]
  public async Task GetPublicationsByDomainChartAsync_GroupsByDomain()
  {
    var result = await _repository.GetPublicationsByDomainChartAsync();

    Assert.Equal("pie", result.ChartType);
    Assert.Equal(2, result.Labels.Count); // ComputerScience + ArtificialIntelligence
    Assert.Equal(6, result.Values.Sum());
  }

  [Fact]
  public async Task GetTopJournalsChartAsync_ReturnsJournalCounts()
  {
    var result = await _repository.GetTopJournalsChartAsync(5);

    Assert.Equal("horizontalBar", result.ChartType);
    Assert.NotEmpty(result.Labels);
    Assert.Equal("AI Journal", result.Labels[0]);
    Assert.True(result.Values[0] > result.Values[^1]);
  }
}

public class TrendAnalyticsRepositoryEmptyDbTests
{
  [Fact]
  public async Task GetDashboardSummaryAsync_EmptyDatabase_ReturnsZeros()
  {
    await using var context = TestDbContextFactory.Create();
    var repository = new TrendAnalyticsRepository(context);

    var result = await repository.GetDashboardSummaryAsync();

    Assert.Equal(0, result.TotalPapers);
    Assert.Equal(0, result.PapersThisYear);
    Assert.Equal(0, result.TotalJournals);
    Assert.Equal(0, result.AverageCitations);
  }

  [Fact]
  public async Task GetTrendingTopicsAsync_EmptyDatabase_ReturnsEmptyItems()
  {
    await using var context = TestDbContextFactory.Create();
    var repository = new TrendAnalyticsRepository(context);

    var result = await repository.GetTrendingTopicsAsync(10, 2);

    Assert.Empty(result.Items);
  }
}
