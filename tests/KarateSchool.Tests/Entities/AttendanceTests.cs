using KarateSchool.Web.Models.Entities;
using Xunit;

namespace KarateSchool.Tests.Entities;

public class AttendanceTests
{
    private static (Student student, KarateClass karateClass) CreateFixtures()
    {
        var instructor = new Instructor(
            "Instr", "instr-att2@example.com", "hash", "555-0000", "Karate", "Black Belt", new DateTime(2020, 1, 1));
        var student = new Student(
            "Student", "student-att@example.com", "hash", "555-0000",
            new DateTime(1990, 1, 1), "White Belt", DateTime.UtcNow.Date, null);
        var karateClass = new KarateClass("Class", "Mon 10:00", "Room 1", 10, instructor);
        return (student, karateClass);
    }

    [Fact]
    public void Constructor_FutureDate_ThrowsArgumentException()
    {
        var (student, karateClass) = CreateFixtures();

        var ex = Assert.Throws<ArgumentException>(() =>
            new Attendance(student, karateClass, DateTime.UtcNow.Date.AddDays(1), "Present"));

        Assert.Contains("future", ex.Message);
    }

    [Fact]
    public void Constructor_TodayDate_Succeeds()
    {
        var (student, karateClass) = CreateFixtures();
        var attendance = new Attendance(student, karateClass, DateTime.UtcNow.Date, "Present");
        Assert.Equal("Present", attendance.Status);
    }

    [Fact]
    public void Constructor_InvalidStatus_ThrowsArgumentException()
    {
        var (student, karateClass) = CreateFixtures();
        Assert.Throws<ArgumentException>(() =>
            new Attendance(student, karateClass, DateTime.UtcNow.Date, "Maybe"));
    }
}
