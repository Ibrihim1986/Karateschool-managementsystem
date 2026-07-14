using KarateSchool.Web.Data;
using KarateSchool.Web.Repositories;
using KarateSchool.Web.Services.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KarateSchool.Tests.Repositories;

public class UserRepositoryTests
{
    private static UserRepository CreateRepository(out ApplicationDbContext context)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        context = new ApplicationDbContext(options);
        return new UserRepository(context);
    }

    [Fact]
    public async Task EmailExistsAsync_EnforcesDuplicateIdentityCheck()
    {
        var repository = CreateRepository(out var context);
        var student = UserFactory.CreateStudent(
            "Repo Student", "repo@example.com", "hash", "555-0000",
            new DateTime(1995, 1, 1), "White Belt", DateTime.UtcNow.Date, null);

        await repository.AddAsync(student);
        await repository.SaveChangesAsync();

        Assert.True(await repository.EmailExistsAsync("repo@example.com"));
        Assert.True(await repository.EmailExistsAsync("REPO@EXAMPLE.COM"));
        Assert.False(await repository.EmailExistsAsync("someone-else@example.com"));
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsMatchingUser()
    {
        var repository = CreateRepository(out _);
        var admin = UserFactory.CreateAdministrator(
            "Repo Admin", "repo-admin@example.com", "hash", "555-0000", "Full");

        await repository.AddAsync(admin);
        await repository.SaveChangesAsync();

        var found = await repository.GetByEmailAsync("repo-admin@example.com");

        Assert.NotNull(found);
        Assert.Equal(admin.UserId, found!.UserId);
    }
}
