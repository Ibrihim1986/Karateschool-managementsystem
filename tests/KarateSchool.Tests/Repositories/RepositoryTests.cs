using KarateSchool.Web.Data;
using KarateSchool.Web.Models.Entities;
using KarateSchool.Web.Repositories;
using KarateSchool.Web.Services.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KarateSchool.Tests.Repositories;

public class RepositoryTests
{
    private static Repository<KarateClass> CreateRepository(out ApplicationDbContext context)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        context = new ApplicationDbContext(options);
        return new Repository<KarateClass>(context);
    }

    private static KarateClass CreateClass(ApplicationDbContext context)
    {
        var instructor = UserFactory.CreateInstructor(
            "Repo Instr", $"repo-instr-{Guid.NewGuid():N}@example.com", "hash", "555-0000",
            "Karate", "Black Belt", new DateTime(2020, 1, 1));
        context.Instructors.Add(instructor);
        context.SaveChanges();

        var karateClass = new KarateClass("Repo Class", "Mon 10:00", "Room 1", 10, instructor);
        return karateClass;
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Repository<KarateClass>(null!));
    }

    [Fact]
    public async Task AddAsync_NullEntity_ThrowsArgumentNullException()
    {
        var repository = CreateRepository(out _);
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null!));
    }

    [Fact]
    public void Update_NullEntity_ThrowsArgumentNullException()
    {
        var repository = CreateRepository(out _);
        Assert.Throws<ArgumentNullException>(() => repository.Update(null!));
    }

    [Fact]
    public void Remove_NullEntity_ThrowsArgumentNullException()
    {
        var repository = CreateRepository(out _);
        Assert.Throws<ArgumentNullException>(() => repository.Remove(null!));
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsEntity()
    {
        var repository = CreateRepository(out var context);
        var karateClass = CreateClass(context);

        await repository.AddAsync(karateClass);
        await repository.SaveChangesAsync();

        var found = await repository.GetByIdAsync(karateClass.ClassId);
        Assert.NotNull(found);
        Assert.Equal("Repo Class", found!.ClassName);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        var repository = CreateRepository(out var context);
        var karateClass = CreateClass(context);
        await repository.AddAsync(karateClass);
        await repository.SaveChangesAsync();

        var all = await repository.GetAllAsync();

        Assert.Single(all);
    }

    [Fact]
    public async Task Remove_DeletesEntity()
    {
        var repository = CreateRepository(out var context);
        var karateClass = CreateClass(context);
        await repository.AddAsync(karateClass);
        await repository.SaveChangesAsync();

        repository.Remove(karateClass);
        await repository.SaveChangesAsync();

        var all = await repository.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var repository = CreateRepository(out var context);
        var karateClass = CreateClass(context);
        await repository.AddAsync(karateClass);
        await repository.SaveChangesAsync();

        repository.Update(karateClass);
        await repository.SaveChangesAsync();

        var found = await repository.GetByIdAsync(karateClass.ClassId);
        Assert.NotNull(found);
    }
}
