using KarateSchool.Web.Models.Entities;
using Xunit;

namespace KarateSchool.Tests.Entities;

public class UserTests
{
    private static Instructor CreateValidInstructor() =>
        new("Test Instructor", "instructor@example.com", "hash", "555-0000",
            "Karate", "Black Belt", new DateTime(2020, 1, 1));

    [Fact]
    public void Constructor_NormalizesEmailToLowercase()
    {
        var instructor = new Instructor(
            "Test Instructor", "MixedCase@Example.com", "hash", "555-0000",
            "Karate", "Black Belt", new DateTime(2020, 1, 1));

        Assert.Equal("mixedcase@example.com", instructor.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign.com")]
    public void Constructor_InvalidEmail_ThrowsArgumentException(string invalidEmail)
    {
        Assert.Throws<ArgumentException>(() =>
            new Instructor("Test", invalidEmail, "hash", "555-0000", "Karate", "Black Belt", new DateTime(2020, 1, 1)));
    }

    [Fact]
    public void Constructor_EmptyFullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Instructor("", "instructor@example.com", "hash", "555-0000", "Karate", "Black Belt", new DateTime(2020, 1, 1)));
    }

    [Fact]
    public void SetPasswordHash_Null_ThrowsArgumentNullException()
    {
        var instructor = CreateValidInstructor();
        Assert.Throws<ArgumentNullException>(() => instructor.SetPasswordHash(null!));
    }

    [Fact]
    public void RegisterFailedLogin_LocksAccountAfterFiveAttempts()
    {
        var user = CreateValidInstructor();

        for (var i = 0; i < User.MaxFailedLoginAttempts - 1; i++)
        {
            user.RegisterFailedLogin();
            Assert.False(user.IsLockedOut);
        }

        user.RegisterFailedLogin();

        Assert.True(user.IsLockedOut);
        Assert.NotNull(user.LockoutEnd);
        Assert.True(user.LockoutEnd > DateTime.UtcNow);
    }

    [Fact]
    public void RegisterFailedLogin_DoesNotExtendLockoutOnceLocked()
    {
        var user = CreateValidInstructor();
        for (var i = 0; i < User.MaxFailedLoginAttempts; i++)
            user.RegisterFailedLogin();

        var firstLockoutEnd = user.LockoutEnd;
        user.RegisterFailedLogin();

        Assert.Equal(firstLockoutEnd, user.LockoutEnd);
    }

    [Fact]
    public void ResetFailedLogins_ClearsCounterAndLockout()
    {
        var user = CreateValidInstructor();
        for (var i = 0; i < User.MaxFailedLoginAttempts; i++)
            user.RegisterFailedLogin();

        user.ResetFailedLogins();

        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
        Assert.False(user.IsLockedOut);
    }

    [Fact]
    public void ToString_IncludesRoleNameAndEmail()
    {
        var user = CreateValidInstructor();
        var text = user.ToString();

        Assert.Contains("Instructor", text);
        Assert.Contains(user.FullName, text);
        Assert.Contains(user.Email, text);
    }
}
