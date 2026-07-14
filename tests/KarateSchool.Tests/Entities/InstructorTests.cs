using KarateSchool.Web.Data;
using KarateSchool.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KarateSchool.Tests.Entities;

public class InstructorTests
{
    private static Instructor CreateInstructor() =>
        new("Test Instructor", "instr-test@example.com", "hash", "555-0000",
            "Karate", "Black Belt", new DateTime(2020, 1, 1));

    [Fact]
    public void Constructor_FutureHireDate_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Instructor("Test", "instr2@example.com", "hash", "555-0000",
                "Karate", "Black Belt", DateTime.UtcNow.Date.AddDays(1)));
    }

    [Fact]
    public void Constructor_EmptySpecialty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Instructor("Test", "instr3@example.com", "hash", "555-0000",
                "", "Black Belt", new DateTime(2020, 1, 1)));
    }

    [Fact]
    public void Constructor_EmptyCertification_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Instructor("Test", "instr4@example.com", "hash", "555-0000",
                "Karate", "", new DateTime(2020, 1, 1)));
    }

    [Fact]
    public void InstructorId_MatchesUserId()
    {
        var instructor = CreateInstructor();
        Assert.Equal(instructor.UserId, instructor.InstructorId);
    }

    [Fact]
    public void IsAssignedTo_NullClass_ThrowsArgumentNullException()
    {
        var instructor = CreateInstructor();
        Assert.Throws<ArgumentNullException>(() => instructor.IsAssignedTo(null!));
    }

    [Fact]
    public void IsAssignedTo_OwnClass_ReturnsTrue()
    {
        var instructor = CreateInstructor();
        var karateClass = new KarateClass("Class", "Mon 10:00", "Room 1", 10, instructor);

        Assert.True(instructor.IsAssignedTo(karateClass));
    }

    [Fact]
    public void IsAssignedTo_OtherInstructorsClass_ReturnsFalse()
    {
        // Both instructors need real, distinct auto-generated IDs for this comparison to be
        // meaningful — unsaved entities all default to UserId 0, which would make any two
        // in-memory instructors compare as "the same" instructor.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new ApplicationDbContext(options);

        var instructor = CreateInstructor();
        var otherInstructor = new Instructor(
            "Other", "instr-other@example.com", "hash", "555-0000",
            "Judo", "3rd Dan", new DateTime(2019, 1, 1));
        context.Instructors.AddRange(instructor, otherInstructor);
        context.SaveChanges();

        var karateClass = new KarateClass("Class", "Mon 10:00", "Room 1", 10, otherInstructor);

        Assert.False(instructor.IsAssignedTo(karateClass));
    }

    [Fact]
    public void ToString_IncludesSpecialtyAndCertification()
    {
        var instructor = CreateInstructor();
        var text = instructor.ToString();

        Assert.Contains("Karate", text);
        Assert.Contains("Black Belt", text);
    }
}
