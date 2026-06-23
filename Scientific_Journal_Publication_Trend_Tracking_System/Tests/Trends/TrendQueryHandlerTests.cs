using FluentValidation;
using MediatR;
using Moq;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Handlers;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Validators;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Shared.Behaviors;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Tests.Trends;

public class TrendQueryHandlerTests
{
  [Fact]
  public async Task GetKeywordTrendQueryHandler_DelegatesToRepository()
  {
    var mockRepo = new Mock<ITrendAnalyticsRepository>();
    var expected = new KeywordTrendResponse
    {
      Keyword = "nlp",
      TotalPublications = 5
    };

    mockRepo
        .Setup(r => r.GetKeywordTrendAsync("nlp", 2020, 2024, It.IsAny<CancellationToken>()))
        .ReturnsAsync(expected);

    var handler = new GetKeywordTrendQueryHandler(mockRepo.Object);
    var result = await handler.Handle(
        new GetKeywordTrendQuery("nlp", 2020, 2024),
        CancellationToken.None);

    Assert.Equal(expected, result);
    mockRepo.Verify(r => r.GetKeywordTrendAsync("nlp", 2020, 2024, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetTrendingTopicsQueryHandler_DelegatesToRepository()
  {
    var mockRepo = new Mock<ITrendAnalyticsRepository>();
    var expected = new TrendingTopicsResponse
    {
      Items = [new TrendingTopicItemDto { Rank = 1, Name = "AI", Type = "Keyword" }]
    };

    mockRepo
        .Setup(r => r.GetTrendingTopicsAsync(5, 2, It.IsAny<CancellationToken>()))
        .ReturnsAsync(expected);

    var handler = new GetTrendingTopicsQueryHandler(mockRepo.Object);
    var result = await handler.Handle(
        new GetTrendingTopicsQuery(5, 2),
        CancellationToken.None);

    Assert.Single(result.Items);
    Assert.Equal("AI", result.Items[0].Name);
  }
}

public class ValidationBehaviorTests
{
  [Fact]
  public async Task ValidationBehavior_InvalidQuery_ThrowsValidationException()
  {
    var validators = new IValidator<GetTrendingTopicsQuery>[]
    {
      new GetTrendingTopicsQueryValidator()
    };

    var behavior = new ValidationBehavior<GetTrendingTopicsQuery, TrendingTopicsResponse>(validators);

    await Assert.ThrowsAsync<ValidationException>(() =>
        behavior.Handle(
            new GetTrendingTopicsQuery(999, 2),
            () => Task.FromResult(new TrendingTopicsResponse()),
            CancellationToken.None));
  }

  [Fact]
  public async Task ValidationBehavior_ValidQuery_CallsNext()
  {
    var validators = new IValidator<GetTrendingTopicsQuery>[]
    {
      new GetTrendingTopicsQueryValidator()
    };

    var behavior = new ValidationBehavior<GetTrendingTopicsQuery, TrendingTopicsResponse>(validators);
    var expected = new TrendingTopicsResponse { Items = [] };

    var result = await behavior.Handle(
        new GetTrendingTopicsQuery(10, 2),
        () => Task.FromResult(expected),
        CancellationToken.None);

    Assert.Same(expected, result);
  }
}
