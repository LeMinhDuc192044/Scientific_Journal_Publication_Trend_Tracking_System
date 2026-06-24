using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.Queries;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.Validators;

public class GetPublicationsByYearChartQueryValidator : AbstractValidator<GetPublicationsByYearChartQuery>
{
    public GetPublicationsByYearChartQueryValidator()
    {
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

public class GetTopKeywordsChartQueryValidator : AbstractValidator<GetTopKeywordsChartQuery>
{
    public GetTopKeywordsChartQueryValidator()
    {
        RuleFor(x => x.TopCount)
            .InclusiveBetween(1, 50)
            .WithMessage("Top count must be between 1 and 50");
    }
}

public class GetTopJournalsChartQueryValidator : AbstractValidator<GetTopJournalsChartQuery>
{
    public GetTopJournalsChartQueryValidator()
    {
        RuleFor(x => x.TopCount)
            .InclusiveBetween(1, 50)
            .WithMessage("Top count must be between 1 and 50");
    }
}
