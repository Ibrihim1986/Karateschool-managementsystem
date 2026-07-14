using KarateSchool.Web.Data;
using KarateSchool.Web.Models.Entities;
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

    [Fact]
    public async Task GetByEmailAsync_EmptyEmail_ThrowsArgumentException()
    {
        var repository = CreateRepository(out _);
        await Assert.ThrowsAsync<ArgumentException>(() => repository.GetByEmailAsync(""));
    }

    [Fact]
    public async Task EmailExistsAsync_EmptyEmail_ThrowsArgumentException()
    {
        var repository = CreateRepository(out _);
        await Assert.ThrowsAsync<ArgumentException>(() => repository.EmailExistsAsync(""));
    }

    [Fact]
    public async Task SearchStudentsAsync_NoSearchTerm_ReturnsAllStudents()
    {
        var repository = CreateRepository(out var context);
        var student1 = UserFactory.CreateStudent(
            "Search Alpha", "search-alpha@example.com", "hash", "555-0000",
            new DateTime(1995, 1, 1), "White Belt", DateTime.UtcNow.Date, null);
        var student2 = UserFactory.CreateStudent(
            "Search Beta", "search-beta@example.com", "hash", "555-0000",
            new DateTime(1995, 1, 1), "White Belt", DateTime.UtcNow.Date, null);
        context.Students.AddRange(student1, student2);
        await context.SaveChangesAsync();

        var results = await repository.SearchStudentsAsync(null);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task SearchStudentsAsync_ById_ReturnsMatchingStudent()
    {
        var repository = CreateRepository(out var context);
        var student = UserFactory.CreateStudent(
            "Search ById", "search-byid@example.com", "hash", "555-0000",
            new DateTime(1995, 1, 1), "White Belt", DateTime.UtcNow.Date, null);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var results = await repository.SearchStudentsAsync(student.UserId.ToString());

        Assert.Single(results);
        Assert.Equal(student.UserId, results[0].UserId);
    }

    [Fact]
    public async Task SearchStudentsAsync_ByName_ReturnsMatchingStudent()
    {
        var repository = CreateRepository(out var context);
        var student = UserFactory.CreateStudent(
            "Unique Search Name", "search-byname@example.com", "hash", "555-0000",
            new DateTime(1995, 1, 1), "White Belt", DateTime.UtcNow.Date, null);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var results = await repository.SearchStudentsAsync("Unique Search");

        Assert.Single(results);
    }

    [Fact]
    public async Task SearchStudentsAsync_ByClassName_ReturnsEnrolledStudent()
    {
        var repository = CreateRepository(out var context);
        var instructor = UserFactory.CreateInstructor(
            "Search Instr", "search-instr@example.com", "hash", "555-0000",
            "Karate", "Black Belt", new DateTime(2020, 1, 1));
        var student = UserFactory.CreateStudent(
            "Class Search Student", "search-class-student@example.com", "hash", "555-0000",
            new DateTime(1995, 1, 1), "White Belt", DateTime.UtcNow.Date, null);
        context.Instructors.Add(instructor);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var karateClass = new KarateClass("Searchable Class", "Fri 10:00", "Room 9", 5, instructor);
        context.KarateClasses.Add(karateClass);
        await context.SaveChangesAsync();

        context.Enrollments.Add(new Enrollment(student, karateClass, DateTime.UtcNow.Date));
        await context.SaveChangesAsync();

        var results = await repository.SearchStudentsAsync("Searchable");

        Assert.Single(results);
        Assert.Equal(student.UserId, results[0].UserId);
    }

    [Fact]
    public async Task SearchStudentsAsync_NoMatch_ReturnsEmpty()
    {
        var repository = CreateRepository(out var context);
        var student = UserFactory.CreateStudent(
            "No Match Student", "search-nomatch@example.com", "hash", "555-0000",
            new DateTime(1995, 1, 1), "White Belt", DateTime.UtcNow.Date, null);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var results = await repository.SearchStudentsAsync("Nonexistent Name Xyz");

        Assert.Empty(results);
    }
}
