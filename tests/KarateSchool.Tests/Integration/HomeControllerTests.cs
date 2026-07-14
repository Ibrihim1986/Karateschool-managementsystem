using System.Diagnostics;
using System.Net;
using Xunit;

namespace KarateSchool.Tests.Integration;

public class HomeControllerTests : IClassFixture<KarateSchoolWebApplicationFactory>
{
    private readonly KarateSchoolWebApplicationFactory _factory;

    public HomeControllerTests(KarateSchoolWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Index_ReturnsSuccess()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Privacy_ReturnsSuccess()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Home/Privacy");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Error_ReturnsSuccess()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Home/Error");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Error_WithActiveActivity_UsesActivityIdAsRequestId()
    {
        // `Activity.Current?.Id ?? HttpContext.TraceIdentifier` has two independent null-checks:
        // whether an Activity is current at all, and whether its Id was assigned. Without an
        // explicit Activity (the other Error test), both checks fall through to TraceIdentifier.
        // Starting a real, formatted root Activity here — and keeping it flowing via AsyncLocal
        // across the in-process TestServer call — exercises the "Id is present" side of both.
        using var activity = new Activity("KarateSchool.Tests.HomeController.Error");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();
        try
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/Home/Error");

            response.EnsureSuccessStatusCode();
        }
        finally
        {
            activity.Stop();
        }
    }

    [Fact]
    public async Task UnknownRoute_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/this-route-does-not-exist");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
