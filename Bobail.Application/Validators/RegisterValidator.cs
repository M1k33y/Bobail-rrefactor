using FluentValidation;

public class RegisterValidator : AbstractValidator<(string Email, string Password, string Nickname)>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email required")
            .EmailAddress().WithMessage("Invalid email");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6).WithMessage("Password too short");

        RuleFor(x => x.Nickname)
            .NotEmpty()
            .MinimumLength(3).WithMessage("Invalid nickname");
    }
}