using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Validators;

public class UpdateResearchTopicCommandValidator : AbstractValidator<UpdateResearchTopicCommand>
{
    public UpdateResearchTopicCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Domain).IsInEnum();
    }
}