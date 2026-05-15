using FluentValidation;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Constants;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Validators;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.EmailRequired)
            .EmailAddress().WithMessage(ValidationMessages.EmailInvalid);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(ValidationMessages.FullNameRequired)
            .MinimumLength(2).WithMessage(ValidationMessages.FullNameTooShort)
            .MaximumLength(100).WithMessage(ValidationMessages.FullNameTooLong);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.PasswordRequired)
            .MinimumLength(8).WithMessage(ValidationMessages.PasswordTooShort)
            .Matches("[A-Z]").WithMessage(ValidationMessages.PasswordRequiresUppercase)
            .Matches("[a-z]").WithMessage(ValidationMessages.PasswordRequiresLowercase)
            .Matches("[0-9]").WithMessage(ValidationMessages.PasswordRequiresDigit)
            .Matches("[!@#$%^&*]").WithMessage(ValidationMessages.PasswordRequiresSpecialChar);
    }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.EmailRequired)
            .EmailAddress().WithMessage(ValidationMessages.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.PasswordRequired);
    }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(ValidationMessages.RefreshTokenRequired);
    }
}
