using System.Text.RegularExpressions;

namespace Bobail.Application.Validators;

//Am nevoie de un PasswordPolicy sa nu am 100 de regexuri si sa unesc inconsistentele cu frontul peste tot reset/change password
public static partial class PasswordPolicy
{
    public const int MinimumLength = 8;
    public const string PasswordRequirementsMessage =
        "Password must be at least 8 characters and include at least one uppercase letter, one lowercase letter, and one digit";

    public static bool IsValid(string? password)
    {
        return !string.IsNullOrWhiteSpace(password)
            && password.Trim().Length >= MinimumLength
            && PasswordRegex().IsMatch(password.Trim());
    }

    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")]
    private static partial Regex PasswordRegex();
}
