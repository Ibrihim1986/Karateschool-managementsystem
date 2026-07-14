using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace KarateSchool.Tests.Integration;

public class AccountControllerTests : IClassFixture<KarateSchoolWebApplicationFactory>
{
    private readonly KarateSchoolWebApplicationFactory _factory;

    public AccountControllerTests(KarateSchoolWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Login_Get_ReturnsSuccess()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/Account/Login");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Register_Get_ReturnsSuccess()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/Account/Register");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AccessDenied_Get_ReturnsSuccess()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/Account/AccessDenied");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RegisterConfirmation_Get_ReturnsSuccess()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/Account/RegisterConfirmation");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_ValidCredentials_RedirectsHomeAndSetsAuthCookie()
    {
        var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        var token = await AntiForgeryHelper.ExtractTokenAsync(loginPage);

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "instructor2@karateschool.com",
            ["Password"] = "Passw0rd!",
            ["__RequestVerificationToken"] = token,
        }));

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie" && h.Value.Any(v => v.Contains(".AspNetCore.Cookies")));
    }

    [Fact]
    public async Task Login_InvalidPassword_ReRendersWithError()
    {
        var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        var token = await AntiForgeryHelper.ExtractTokenAsync(loginPage);

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "student2@karateschool.com",
            ["Password"] = "WrongPassword!",
            ["__RequestVerificationToken"] = token,
        }));

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid email or password", html);
    }

    [Fact]
    public async Task Login_MissingFields_ReturnsValidationError()
    {
        var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        var token = await AntiForgeryHelper.ExtractTokenAsync(loginPage);

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "",
            ["Password"] = "",
            ["__RequestVerificationToken"] = token,
        }));

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("required", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_FiveFailedAttempts_LocksAccount()
    {
        var client = CreateClient();
        HttpResponseMessage response = null!;

        for (var i = 0; i < 5; i++)
        {
            var loginPage = await client.GetAsync("/Account/Login");
            var token = await AntiForgeryHelper.ExtractTokenAsync(loginPage);

            response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "student3@karateschool.com",
                ["Password"] = "WrongPassword!",
                ["__RequestVerificationToken"] = token,
            }));
        }

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("locked", html, StringComparison.OrdinalIgnoreCase);

        // Even the correct password is now rejected until the lockout expires.
        var loginPage2 = await client.GetAsync("/Account/Login");
        var token2 = await AntiForgeryHelper.ExtractTokenAsync(loginPage2);
        var correctAttempt = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "student3@karateschool.com",
            ["Password"] = "Passw0rd!",
            ["__RequestVerificationToken"] = token2,
        }));
        var html2 = await correctAttempt.Content.ReadAsStringAsync();
        Assert.Contains("locked", html2, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_ValidAdult_RedirectsToConfirmation()
    {
        var client = CreateClient();
        var registerPage = await client.GetAsync("/Account/Register");
        var token = await AntiForgeryHelper.ExtractTokenAsync(registerPage);

        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FullName"] = "Integration Test Student",
            ["Email"] = $"itest-{Guid.NewGuid():N}@example.com",
            ["Phone"] = "555-1212",
            ["Password"] = "Str0ng!Passw0rd",
            ["ConfirmPassword"] = "Str0ng!Passw0rd",
            ["DateOfBirth"] = "1995-06-15",
            ["EmergencyContact"] = "",
            ["__RequestVerificationToken"] = token,
        }));

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Account/RegisterConfirmation", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Register_MinorWithoutGuardian_ReRendersWithError()
    {
        var client = CreateClient();
        var registerPage = await client.GetAsync("/Account/Register");
        var token = await AntiForgeryHelper.ExtractTokenAsync(registerPage);

        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FullName"] = "Minor No Guardian",
            ["Email"] = $"minor-{Guid.NewGuid():N}@example.com",
            ["Phone"] = "555-1212",
            ["Password"] = "Str0ng!Passw0rd",
            ["ConfirmPassword"] = "Str0ng!Passw0rd",
            ["DateOfBirth"] = DateTime.UtcNow.Date.AddYears(-10).ToString("yyyy-MM-dd"),
            ["EmergencyContact"] = "",
            ["__RequestVerificationToken"] = token,
        }));

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("guardian", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReRendersWithError()
    {
        var client = CreateClient();
        var registerPage = await client.GetAsync("/Account/Register");
        var token = await AntiForgeryHelper.ExtractTokenAsync(registerPage);

        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FullName"] = "Duplicate Email",
            ["Email"] = "admin@karateschool.com",
            ["Phone"] = "555-1212",
            ["Password"] = "Str0ng!Passw0rd",
            ["ConfirmPassword"] = "Str0ng!Passw0rd",
            ["DateOfBirth"] = "1990-01-01",
            ["EmergencyContact"] = "",
            ["__RequestVerificationToken"] = token,
        }));

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("already exists", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsValidationError()
    {
        var client = CreateClient();
        var registerPage = await client.GetAsync("/Account/Register");
        var token = await AntiForgeryHelper.ExtractTokenAsync(registerPage);

        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FullName"] = "Weak Password",
            ["Email"] = $"weak-{Guid.NewGuid():N}@example.com",
            ["Phone"] = "555-1212",
            ["Password"] = "weak",
            ["ConfirmPassword"] = "weak",
            ["DateOfBirth"] = "1990-01-01",
            ["EmergencyContact"] = "",
            ["__RequestVerificationToken"] = token,
        }));

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("least 8 characters", html);
    }

    [Fact]
    public async Task Register_EmailPassesAnnotationButFailsDomainValidation_ReRendersWithError()
    {
        // [EmailAddress] only requires a single '@' with non-whitespace on both sides, so an
        // address with no dot (e.g. a bare hostname) passes it but is rejected by the stricter
        // domain-level check in User.SetEmail — this is the one path that reaches AccountController's
        // catch (ArgumentException) branch, since RegisterStudentViewModel's own password regex
        // otherwise mirrors PasswordPolicy exactly and never diverges from it.
        var client = CreateClient();
        var registerPage = await client.GetAsync("/Account/Register");
        var token = await AntiForgeryHelper.ExtractTokenAsync(registerPage);

        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FullName"] = "Bad Domain Email",
            ["Email"] = "student@localhost",
            ["Phone"] = "555-1212",
            ["Password"] = "Str0ng!Passw0rd",
            ["ConfirmPassword"] = "Str0ng!Passw0rd",
            ["DateOfBirth"] = "1990-01-01",
            ["EmergencyContact"] = "",
            ["__RequestVerificationToken"] = token,
        }));

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("not a valid email address", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Logout_Post_RedirectsHome()
    {
        var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        var token = await AntiForgeryHelper.ExtractTokenAsync(loginPage);
        await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "instructor1@karateschool.com",
            ["Password"] = "Passw0rd!",
            ["__RequestVerificationToken"] = token,
        }));

        var homePage = await client.GetAsync("/");
        var logoutToken = await AntiForgeryHelper.ExtractTokenAsync(homePage);

        var response = await client.PostAsync("/Account/Logout", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = logoutToken,
        }));

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnUrl_RedirectsToLocalUrlAfterSuccess()
    {
        var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login?returnUrl=%2FHome%2FPrivacy");
        var token = await AntiForgeryHelper.ExtractTokenAsync(loginPage);

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "admin@karateschool.com",
            ["Password"] = "Passw0rd!",
            ["ReturnUrl"] = "/Home/Privacy",
            ["__RequestVerificationToken"] = token,
        }));

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Home/Privacy", response.Headers.Location?.ToString());
    }
}
