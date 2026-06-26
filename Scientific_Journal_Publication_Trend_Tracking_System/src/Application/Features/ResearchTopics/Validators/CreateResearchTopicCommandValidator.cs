using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Validators
{
    public class CreateResearchTopicCommandValidator : AbstractValidator<CreateResearchTopicCommand>
    {
        public CreateResearchTopicCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Topic name is required")
                .MaximumLength(255);

            RuleFor(x => x.Description)
                .MaximumLength(2000);

            RuleFor(x => x.Domain)
                .IsInEnum().WithMessage("Invalid research domain");
        }
    }
}
