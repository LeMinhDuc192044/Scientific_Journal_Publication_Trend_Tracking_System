using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.Validators;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Validators;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Tests.Trends;

public class TrendValidatorsTests
{
  private readonly GetKeywordTrendQueryValidator _keywordValidator = new();
  private readonly GetTopicTrendQueryValidator _topicValidator = new();
  private readonly GetTrendingTopicsQueryValidator _trendingValidator = new();

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("a")]
  public void GetKeywordTrendQuery_InvalidKeyword_ShouldFail(string keyword)
  {
    var result = _keywordValidator.Validate(new GetKeywordTrendQuery(keyword));
    Assert.False(result.IsValid);
  }

  [Fact]
  public void GetKeywordTrendQuery_ValidInput_ShouldPass()
  {
    var result = _keywordValidator.Validate(
        new GetKeywordTrendQuery("machine learning", 2020, 2024));

    Assert.True(result.IsValid);
  }

  [Fact]
  public void GetKeywordTrendQuery_FromYearGreaterThanToYear_ShouldFail()
  {
    var result = _keywordValidator.Validate(
        new GetKeywordTrendQuery("machine learning", 2024, 2020));

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("From year cannot be greater"));
  }

  [Theory]
  [InlineData(1899)]
  [InlineData(2101)]
  public void GetKeywordTrendQuery_InvalidYearRange_ShouldFail(int year)
  {
    var result = _keywordValidator.Validate(
        new GetKeywordTrendQuery("machine learning", year, null));

    Assert.False(result.IsValid);
  }

  [Fact]
  public void GetTopicTrendQuery_EmptyTopicId_ShouldFail()
  {
    var result = _topicValidator.Validate(
        new GetTopicTrendQuery(Guid.Empty, 2020, 2024));

    Assert.False(result.IsValid);
  }

  [Fact]
  public void GetTopicTrendQuery_ValidInput_ShouldPass()
  {
    var result = _topicValidator.Validate(
        new GetTopicTrendQuery(Guid.NewGuid(), 2020, 2024));

    Assert.True(result.IsValid);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(51)]
  [InlineData(999)]
  public void GetTrendingTopicsQuery_InvalidTopCount_ShouldFail(int topCount)
  {
    var result = _trendingValidator.Validate(
        new GetTrendingTopicsQuery(topCount, 2));

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetTrendingTopicsQuery.TopCount));
  }

  [Theory]
  [InlineData(0)]
  [InlineData(6)]
  public void GetTrendingTopicsQuery_InvalidRecentYears_ShouldFail(int recentYears)
  {
    var result = _trendingValidator.Validate(
        new GetTrendingTopicsQuery(10, recentYears));

    Assert.False(result.IsValid);
  }

  [Fact]
  public void GetTrendingTopicsQuery_ValidInput_ShouldPass()
  {
    var result = _trendingValidator.Validate(new GetTrendingTopicsQuery(10, 2));
    Assert.True(result.IsValid);
  }
}

public class DashboardValidatorsTests
{
  private readonly GetPublicationsByYearChartQueryValidator _yearChartValidator = new();
  private readonly GetTopKeywordsChartQueryValidator _keywordsChartValidator = new();
  private readonly GetTopJournalsChartQueryValidator _journalsChartValidator = new();

  [Fact]
  public void GetPublicationsByYearChartQuery_InvalidYearRange_ShouldFail()
  {
    var result = _yearChartValidator.Validate(
        new GetPublicationsByYearChartQuery(2024, 2020));

    Assert.False(result.IsValid);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(100)]
  public void GetTopKeywordsChartQuery_InvalidTopCount_ShouldFail(int topCount)
  {
    var result = _keywordsChartValidator.Validate(
        new GetTopKeywordsChartQuery(topCount));

    Assert.False(result.IsValid);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(100)]
  public void GetTopJournalsChartQuery_InvalidTopCount_ShouldFail(int topCount)
  {
    var result = _journalsChartValidator.Validate(
        new GetTopJournalsChartQuery(topCount));

    Assert.False(result.IsValid);
  }

  [Fact]
  public void GetTopKeywordsChartQuery_ValidInput_ShouldPass()
  {
    var result = _keywordsChartValidator.Validate(new GetTopKeywordsChartQuery(10));
    Assert.True(result.IsValid);
  }
}
