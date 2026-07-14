using KarateSchool.Web.Data;
using KarateSchool.Web.Services.Auth;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KarateSchool.Tests.Data;

public class DbSeederTests
{
    private static ApplicationDbContext CreateSqliteContext(out SqliteConnection connection)
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void Seed_PopulatesUsersClassesAndAnnouncements()
    {
        using var context = CreateSqliteContext(out var connection);
        try
        {
            DbSeeder.Seed(context, new Rfc2898PasswordHasher());

            Assert.True(context.Users.Any());
            Assert.True(context.KarateClasses.Any());
            Assert.True(context.Enrollments.Any());
            Assert.True(context.Attendances.Any());
            Assert.True(context.BeltPromotions.Any());
            Assert.True(context.Payments.Any());
            Assert.True(context.Announcements.Any());
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public void Seed_CalledTwice_DoesNotDuplicateData()
    {
        using var context = CreateSqliteContext(out var connection);
        try
        {
            var hasher = new Rfc2898PasswordHasher();
            DbSeeder.Seed(context, hasher);
            var firstCount = context.Users.Count();

            DbSeeder.Seed(context, hasher);
            var secondCount = context.Users.Count();

            Assert.Equal(firstCount, secondCount);
        }
        finally
        {
            connection.Dispose();
        }
    }
}
