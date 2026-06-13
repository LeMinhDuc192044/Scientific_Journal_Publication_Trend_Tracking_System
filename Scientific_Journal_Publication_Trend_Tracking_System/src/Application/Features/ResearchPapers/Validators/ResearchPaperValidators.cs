using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Validators;

/// <summary>
/// Validator for search research papers request
/// </summary>
public class SearchPapersRequestValidator : AbstractValidator<SearchPapersRequest>
{
    public SearchPapersRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.FromYear)
            .LessThanOrEqualTo(DateTime.UtcNow.Year)
            .When(x => x.FromYear.HasValue)
            .WithMessage("From year cannot be in the future");

        RuleFor(x => x.ToYear)
            .LessThanOrEqualTo(DateTime.UtcNow.Year)
            .When(x => x.ToYear.HasValue)
            .WithMessage("To year cannot be in the future");

        RuleFor(x => x)
            .Must(x => !x.FromYear.HasValue || !x.ToYear.HasValue || x.FromYear <= x.ToYear)
            .WithMessage("From year cannot be greater than to year");
    }
}

/// <summary>
/// Validator for sync research papers command
/// </summary>
public class SyncResearchPapersCommandValidator : AbstractValidator<SyncResearchPapersCommand>
{
    public SyncResearchPapersCommandValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Query cannot be empty")
            .MinimumLength(2)
            .WithMessage("Query must be at least 2 characters");

        RuleFor(x => x.ApiSource)
            .NotEmpty()
            .WithMessage("API source cannot be empty")
            .Must(x => new[] { "semanticscholar", "openalex", "crossref" }.Contains(x.ToLower()))
            .WithMessage("API source must be one of: semanticscholar, openalex, crossref");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 1000)
            .WithMessage("Limit must be between 1 and 1000");
    }
}
