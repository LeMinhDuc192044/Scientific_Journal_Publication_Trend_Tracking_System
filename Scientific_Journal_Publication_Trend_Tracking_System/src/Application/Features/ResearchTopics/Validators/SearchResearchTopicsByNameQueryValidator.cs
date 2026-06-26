using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Validators;

public class SearchResearchTopicsByNameQueryValidator : AbstractValidator<SearchResearchTopicsByNameQuery>
{
    public SearchResearchTopicsByNameQueryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Search name is required");

        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
