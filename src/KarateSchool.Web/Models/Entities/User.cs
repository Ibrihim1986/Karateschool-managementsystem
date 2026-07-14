using System.Text.RegularExpressions;

namespace KarateSchool.Web.Models.Entities;

/// <summary>
/// Abstract base class shared by all account types (Student, Instructor, Administrator).
/// Persisted as the "Users" table; subclasses are persisted as TPT tables sharing the same key (UserId).
/// </summary>
public abstract class User
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public const int MaxFailedLoginAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public int UserId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    /// <summary>Hashed password. Never store or transmit plain text.</summary>
    public string PasswordHash { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;

    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }

    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    // EF Core materialization constructor.
    protected User()
    {
    }

    protected User(string fullName, string email, string passwordHash, string phone, string role)
    {
        SetFullName(fullName);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        SetPhone(phone);
        SetRole(role);
    }

    public void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));
        if (fullName.Length > 150)
            throw new ArgumentException("Full name cannot exceed 150 characters.", nameof(fullName));
        FullName = fullName.Trim();
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        var normalized = email.Trim().ToLowerInvariant();
        if (!EmailPattern.IsMatch(normalized))
            throw new ArgumentException("Email is not a valid email address.", nameof(email));
        Email = normalized;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentNullException(nameof(passwordHash), "Password hash cannot be empty.");
        PasswordHash = passwordHash;
    }

    public void SetPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number is required.", nameof(phone));
        Phone = phone.Trim();
    }

    internal void SetRole(string role)
    {
        if (role is not ("Student" or "Instructor" or "Administrator"))
            throw new ArgumentException("Role must be Student, Instructor, or Administrator.", nameof(role));
        Role = role;
    }

    public void RegisterFailedLogin()
    {
        if (IsLockedOut)
            return;

        FailedLoginAttempts++;
        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
        }
    }

    public void ResetFailedLogins()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    public override string ToString() => $"{Role}: {FullName} <{Email}>";
}
