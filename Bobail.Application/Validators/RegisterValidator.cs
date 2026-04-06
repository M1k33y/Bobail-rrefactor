using FluentValidation;

public class RegisterValidator : AbstractValidator<(string Email, string Password, string Nickname)>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email required")
            .EmailAddress().WithMessage("Invalid email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password required")
            .Must(PasswordPolicy.IsValid)
            .WithMessage(PasswordPolicy.PasswordRequirementsMessage);

        RuleFor(x => x.Nickname)
            .NotEmpty()
            .MinimumLength(3).WithMessage("Invalid nickname");
    }
}
