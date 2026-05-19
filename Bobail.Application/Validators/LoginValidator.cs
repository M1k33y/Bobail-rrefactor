using FluentValidation;

namespace Bobail.Application.Validators;

public class LoginValidator : AbstractValidator<(string Email, string Password)>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
