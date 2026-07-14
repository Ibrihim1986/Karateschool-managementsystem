using KarateSchool.Web.Models.Entities;

namespace KarateSchool.Web.Services.Auth;

public enum LoginOutcome
{
    Success,
    InvalidCredentials,
    LockedOut
}

public class LoginResult
{
    public required LoginOutcome Outcome { get; init; }
    public User? User { get; init; }
    public DateTime? LockoutEnd { get; init; }

    public static LoginResult Success(User user) => new() { Outcome = LoginOutcome.Success, User = user };

    public static LoginResult Invalid() => new() { Outcome = LoginOutcome.InvalidCredentials };

    public static LoginResult LockedOutUntil(DateTime lockoutEnd) =>
        new() { Outcome = LoginOutcome.LockedOut, LockoutEnd = lockoutEnd };
}
