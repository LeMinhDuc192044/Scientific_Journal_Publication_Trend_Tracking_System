using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Queries;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Validators;

public class GetKeywordTrendQueryValidator : AbstractValidator<GetKeywordTrendQuery>
{
    public GetKeywordTrendQueryValidator()
    {
        RuleFor(x => x.Keyword)
            .NotEmpty()
            .WithMessage("Keyword is required")
            .MinimumLength(2)
            .WithMessage("Keyword must be at least 2 characters");

        RuleFor(x => x.FromYear)
            .InclusiveBetween(1900, 2100)
            .When(x => x.FromYear.HasValue)
            .WithMessage("From year must be between 1900 and 2100");

        RuleFor(x => x.ToYear)
            .InclusiveBetween(1900, 2100)
            .When(x => x.ToYear.HasValue)
            .WithMessage("To year must be between 1900 and 2100");

        RuleFor(x => x)
            .Must(x => !x.FromYear.HasValue || !x.ToYear.HasValue || x.FromYear <= x.ToYear)
            .WithMessage("From year cannot be greater than to year");
    }
}

public class GetTopicTrendQueryValidator : AbstractValidator<GetTopicTrendQuery>
{
    public GetTopicTrendQueryValidator()
    {
        RuleFor(x => x.TopicId)
            .NotEmpty()
            .WithMessage("Topic ID is required");

        RuleFor(x => x.FromYear)
            .InclusiveBetween(1900, 2100)
            .When(x => x.FromYear.HasValue);

        RuleFor(x => x.ToYear)
            .InclusiveBetween(1900, 2100)
            .When(x => x.ToYear.HasValue);

        RuleFor(x => x)
            .Must(x => !x.FromYear.HasValue || !x.ToYear.HasValue || x.FromYear <= x.ToYear)
            .WithMessage("From year cannot be greater than to year");
    }
}

public class GetTrendingTopicsQueryValidator : AbstractValidator<GetTrendingTopicsQuery>
{
    public GetTrendingTopicsQueryValidator()
    {
        RuleFor(x => x.TopCount)
            .InclusiveBetween(1, 50)
            .WithMessage("Top count must be between 1 and 50");

        RuleFor(x => x.RecentYears)
            .InclusiveBetween(1, 5)
            .WithMessage("Recent years must be between 1 and 5");
    }
}
