using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchPapers.Commands;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchPapers.Validators;

public class ImportSingleResearchPaperCommandValidator : AbstractValidator<ImportResearchPaperByLinkCommand>
{
    public ImportSingleResearchPaperCommandValidator()
    {
        RuleFor(x => x.Link)
            .NotEmpty().WithMessage("Link cannot be empty");

        RuleFor(x => x.ApiSource)
            .NotEmpty().WithMessage("API source cannot be empty")
            .Must(x => new[] { "semanticscholar", "openalex", "crossref" }.Contains(x.ToLower()))
            .WithMessage("API source must be one of: semanticscholar, openalex, crossref");

        RuleFor(x => x.ResearchTopicIds)
            .NotNull().WithMessage("ResearchTopicIds must be provided (an empty list is allowed if you don't want to link any topic yet)");
    }
}
