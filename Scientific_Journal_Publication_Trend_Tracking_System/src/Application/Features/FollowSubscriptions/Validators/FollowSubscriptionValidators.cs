using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.Queries;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.Validators;

public sealed class CreateFollowSubscriptionCommandValidator
    : AbstractValidator<CreateFollowSubscriptionCommand>
{
    public CreateFollowSubscriptionCommandValidator()
    {
        RuleFor(x => x.TargetType).IsInEnum();
        RuleFor(x => x.TargetId).NotEmpty();
    }
}

public sealed class UnfollowSubscriptionCommandValidator
    : AbstractValidator<UnfollowSubscriptionCommand>
{
    public UnfollowSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}

public sealed class GetMyFollowSubscriptionsQueryValidator
    : AbstractValidator<GetMyFollowSubscriptionsQuery>
{
    public GetMyFollowSubscriptionsQueryValidator()
    {
        RuleFor(x => x.TargetType)
            .IsInEnum()
            .When(x => x.TargetType.HasValue);

        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
