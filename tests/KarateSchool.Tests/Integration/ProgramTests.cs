using KarateSchool.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KarateSchool.Tests.Integration;

/// <summary>
/// Covers the two environment-conditional branches in Program.cs that the standard
/// KarateSchoolWebApplicationFactory never touches: the $PORT-driven UseUrls call (used by
/// hosts like Render) and the non-Development pipeline (UseExceptionHandler/UseHsts).
/// </summary>
public class ProgramTests
{
    private static WebApplicationFactory<Program> CreateFactory(string environment)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
            });
        });
    }

    [Fact]
    public async Task ProductionEnvironment_UsesExceptionHandlerAndHsts_StillServesRequests()
    {
        using var factory = CreateFactory("Production");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PortEnvironmentVariable_IsHonoredWithoutBreakingStartup()
    {
        Environment.SetEnvironmentVariable("PORT", "58734");
        try
        {
            using var factory = CreateFactory("Development");
            var client = factory.CreateClient();

            var response = await client.GetAsync("/");

            response.EnsureSuccessStatusCode();
        }
        finally
        {
            Environment.SetEnvironmentVariable("PORT", null);
        }
    }
}
