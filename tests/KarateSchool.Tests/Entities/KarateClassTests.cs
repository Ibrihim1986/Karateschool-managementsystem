using KarateSchool.Web.Models.Entities;
using Xunit;

namespace KarateSchool.Tests.Entities;

public class KarateClassTests
{
    private static Instructor CreateInstructor(string email = "instr@example.com") =>
        new("Instr", email, "hash", "555-0000", "Karate", "Black Belt", new DateTime(2020, 1, 1));

    private static Student CreateStudent(string email) =>
        new("Student", email, "hash", "555-0000", new DateTime(1990, 1, 1), "White Belt", DateTime.UtcNow.Date, null);

    [Fact]
    public void Constructor_ZeroCapacity_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new KarateClass("Class", "Mon 10:00", "Room 1", 0, CreateInstructor()));
    }

    [Fact]
    public void Constructor_NullInstructor_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new KarateClass("Class", "Mon 10:00", "Room 1", 10, null!));
    }

    [Fact]
    public void Enroll_AtCapacity_ThrowsInvalidOperationException()
    {
        var karateClass = new KarateClass("Class", "Mon 10:00", "Room 1", 1, CreateInstructor());
        var student1 = CreateStudent("s1@example.com");
        var student2 = CreateStudent("s2@example.com");

        karateClass.Enroll(new Enrollment(student1, karateClass, DateTime.UtcNow.Date));

        Assert.Throws<InvalidOperationException>(() =>
            karateClass.Enroll(new Enrollment(student2, karateClass, DateTime.UtcNow.Date)));
    }

    [Fact]
    public void Enroll_DuplicateStudent_ThrowsInvalidOperationException()
    {
        var karateClass = new KarateClass("Class", "Mon 10:00", "Room 1", 5, CreateInstructor());
        var student = CreateStudent("dup@example.com");

        karateClass.Enroll(new Enrollment(student, karateClass, DateTime.UtcNow.Date));

        Assert.Throws<InvalidOperationException>(() =>
            karateClass.Enroll(new Enrollment(student, karateClass, DateTime.UtcNow.Date)));
    }

    [Fact]
    public void HasAvailableCapacity_ReflectsEnrollmentCount()
    {
        var karateClass = new KarateClass("Class", "Mon 10:00", "Room 1", 1, CreateInstructor());
        Assert.True(karateClass.HasAvailableCapacity);

        karateClass.Enroll(new Enrollment(CreateStudent("s@example.com"), karateClass, DateTime.UtcNow.Date));

        Assert.False(karateClass.HasAvailableCapacity);
    }

    [Fact]
    public void Constructor_EmptyClassName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new KarateClass("", "Mon 10:00", "Room 1", 10, CreateInstructor()));
    }

    [Fact]
    public void Constructor_EmptySchedule_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new KarateClass("Class", "", "Room 1", 10, CreateInstructor()));
    }

    [Fact]
    public void Constructor_EmptyRoom_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new KarateClass("Class", "Mon 10:00", "", 10, CreateInstructor()));
    }

    [Fact]
    public void Enroll_NullEnrollment_ThrowsArgumentNullException()
    {
        var karateClass = new KarateClass("Class", "Mon 10:00", "Room 1", 10, CreateInstructor());
        Assert.Throws<ArgumentNullException>(() => karateClass.Enroll(null!));
    }

    [Fact]
    public void ToString_IncludesClassNameAndRoom()
    {
        var karateClass = new KarateClass("Class", "Mon 10:00", "Room 1", 10, CreateInstructor());
        var text = karateClass.ToString();

        Assert.Contains("Class", text);
        Assert.Contains("Room 1", text);
    }
}
