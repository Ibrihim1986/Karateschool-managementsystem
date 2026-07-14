using KarateSchool.Web.Data;
using KarateSchool.Web.Models.Entities;
using KarateSchool.Web.Models.ViewModels;
using KarateSchool.Web.Repositories;
using KarateSchool.Web.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KarateSchool.Tests.Services;

public class AuthServiceTests
{
    private static AuthService CreateService(out ApplicationDbContext context)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        context = new ApplicationDbContext(options);
        var repository = new UserRepository(context);
        return new AuthService(repository, new Rfc2898PasswordHasher());
    }

    private static RegisterStudentViewModel ValidRegistration(string email = "newstudent@example.com") => new()
    {
        FullName = "New Student",
        Email = email,
        Phone = "555-1234",
        Password = "Str0ng!Passw0rd",
        ConfirmPassword = "Str0ng!Passw0rd",
        DateOfBirth = new DateTime(1995, 1, 1),
        EmergencyContact = null,
    };

    [Fact]
    public async Task RegisterStudentAsync_ValidModel_CreatesStudentWithHashedPassword()
    {
        var service = CreateService(out var context);
        var student = await service.RegisterStudentAsync(ValidRegistration());

        Assert.NotEqual(0, student.UserId);
        Assert.NotEqual("Str0ng!Passw0rd", student.PasswordHash);
        Assert.Single(context.Users);
    }

    [Fact]
    public async Task RegisterStudentAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var service = CreateService(out _);
        await service.RegisterStudentAsync(ValidRegistration("dup@example.com"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterStudentAsync(ValidRegistration("dup@example.com")));
    }

    [Fact]
    public async Task RegisterStudentAsync_MinorWithoutGuardian_ThrowsInvalidOperationException()
    {
        var service = CreateService(out _);
        var model = ValidRegistration("minor@example.com");
        model.DateOfBirth = DateTime.UtcNow.Date.AddYears(-10);
        model.EmergencyContact = null;

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterStudentAsync(model));
    }

    [Fact]
    public async Task LoginAsync_CorrectCredentials_ReturnsSuccess()
    {
        var service = CreateService(out _);
        await service.RegisterStudentAsync(ValidRegistration("login@example.com"));

        var result = await service.LoginAsync("login@example.com", "Str0ng!Passw0rd");

        Assert.Equal(LoginOutcome.Success, result.Outcome);
        Assert.NotNull(result.User);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsInvalidCredentials()
    {
        var service = CreateService(out _);
        var result = await service.LoginAsync("nobody@example.com", "whatever");
        Assert.Equal(LoginOutcome.InvalidCredentials, result.Outcome);
    }

    [Fact]
    public async Task LoginAsync_FiveWrongAttempts_LocksAccount()
    {
        var service = CreateService(out _);
        await service.RegisterStudentAsync(ValidRegistration("lockout@example.com"));

        LoginResult result = null!;
        for (var i = 0; i < User.MaxFailedLoginAttempts; i++)
            result = await service.LoginAsync("lockout@example.com", "WrongPassword1!");

        Assert.Equal(LoginOutcome.LockedOut, result.Outcome);

        // Even the correct password should now be rejected until the lockout expires.
        var correctAttempt = await service.LoginAsync("lockout@example.com", "Str0ng!Passw0rd");
        Assert.Equal(LoginOutcome.LockedOut, correctAttempt.Outcome);
    }

    [Fact]
    public async Task LoginAsync_SuccessfulLogin_ResetsFailedAttemptCounter()
    {
        var service = CreateService(out _);
        await service.RegisterStudentAsync(ValidRegistration("reset@example.com"));

        await service.LoginAsync("reset@example.com", "WrongPassword1!");
        await service.LoginAsync("reset@example.com", "WrongPassword1!");
        var success = await service.LoginAsync("reset@example.com", "Str0ng!Passw0rd");

        Assert.Equal(LoginOutcome.Success, success.Outcome);
        Assert.Equal(0, success.User!.FailedLoginAttempts);
    }
}
