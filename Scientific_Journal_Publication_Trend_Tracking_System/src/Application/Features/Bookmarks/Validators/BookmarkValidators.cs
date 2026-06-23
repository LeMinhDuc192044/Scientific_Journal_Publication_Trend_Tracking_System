using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Validators;

public class CreateBookmarkRequestValidator : AbstractValidator<CreateBookmarkRequest>
{
    public CreateBookmarkRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.ResearchPaperId)
            .NotEmpty()
            .When(x => x.Type == BookmarkType.Paper)
            .WithMessage("ResearchPaperId is required for paper bookmarks.");

        RuleFor(x => x)
            .Must(x => x.KeywordId.HasValue || !string.IsNullOrWhiteSpace(x.KeywordName))
            .When(x => x.Type == BookmarkType.Keyword)
            .WithMessage("KeywordId or KeywordName is required for keyword bookmarks.");

        RuleFor(x => x.JournalId)
            .NotEmpty()
            .When(x => x.Type == BookmarkType.Journal)
            .WithMessage("JournalId is required for journal bookmarks.");

        RuleFor(x => x.ResearchTopicId)
            .NotEmpty()
            .When(x => x.Type == BookmarkType.ResearchTopic)
            .WithMessage("ResearchTopicId is required for research topic bookmarks.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes != null);
    }
}

public class CreateBookmarkCommandValidator : AbstractValidator<CreateBookmarkCommand>
{
    public CreateBookmarkCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.ResearchPaperId)
            .NotEmpty()
            .When(x => x.Type == BookmarkType.Paper);

        RuleFor(x => x)
            .Must(x => x.KeywordId.HasValue || !string.IsNullOrWhiteSpace(x.KeywordName))
            .When(x => x.Type == BookmarkType.Keyword);

        RuleFor(x => x.JournalId)
            .NotEmpty()
            .When(x => x.Type == BookmarkType.Journal);

        RuleFor(x => x.ResearchTopicId)
            .NotEmpty()
            .When(x => x.Type == BookmarkType.ResearchTopic);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes != null);
    }
}

public class UpdateBookmarkRequestValidator : AbstractValidator<UpdateBookmarkRequest>
{
    public UpdateBookmarkRequestValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes != null);
    }
}
