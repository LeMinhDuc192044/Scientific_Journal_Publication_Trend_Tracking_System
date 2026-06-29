using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.Queries;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.Validators;

public sealed class GetJournalsQueryValidator : AbstractValidator<GetJournalsQuery>
{
    public GetJournalsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(200);
    }
}

public sealed class GetResearchTopicsQueryValidator : AbstractValidator<GetResearchTopicsQuery>
{
    public GetResearchTopicsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(200);
    }
}
