using System.Text.RegularExpressions;

namespace KarateSchool.Web.Services.Auth;

/// <summary>Minimum password complexity policy: 8+ chars, upper, lower, digit, special character.</summary>
public static class PasswordPolicy
{
    private static readonly Regex ComplexityPattern = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        RegexOptions.Compiled);

    public static void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));

        if (!ComplexityPattern.IsMatch(password))
            throw new ArgumentException(
                "Password must be at least 8 characters and include an uppercase letter, " +
                "a lowercase letter, a digit, and a special character.",
                nameof(password));
    }
}
