using KarateSchool.Web.Models.Entities;
using Xunit;

namespace KarateSchool.Tests.Entities;

public class EnrollmentTests
{
    private static (Student student, KarateClass karateClass) CreateFixtures()
    {
        var instructor = new Instructor(
            "Enroll Instr", "enroll-instr@example.com", "hash", "555-0000",
            "Karate", "Black Belt", new DateTime(2020, 1, 1));
        var student = new Student(
            "Enroll Student", "enroll-student@example.com", "hash", "555-0000",
            new DateTime(1990, 1, 1), "White Belt", DateTime.UtcNow.Date, null);
        var karateClass = new KarateClass("Enroll Class", "Mon 10:00", "Room 1", 10, instructor);
        return (student, karateClass);
    }

    [Fact]
    public void Constructor_NullStudent_ThrowsArgumentNullException()
    {
        var (_, karateClass) = CreateFixtures();
        Assert.Throws<ArgumentNullException>(() =>
            new Enrollment(null!, karateClass, DateTime.UtcNow.Date));
    }

    [Fact]
    public void Constructor_NullKarateClass_ThrowsArgumentNullException()
    {
        var (student, _) = CreateFixtures();
        Assert.Throws<ArgumentNullException>(() =>
            new Enrollment(student, null!, DateTime.UtcNow.Date));
    }

    [Fact]
    public void Constructor_FutureEnrollDate_ThrowsArgumentException()
    {
        var (student, karateClass) = CreateFixtures();
        Assert.Throws<ArgumentException>(() =>
            new Enrollment(student, karateClass, DateTime.UtcNow.Date.AddDays(1)));
    }

    [Fact]
    public void ToString_IncludesStudentAndClassIds()
    {
        var (student, karateClass) = CreateFixtures();
        var enrollment = new Enrollment(student, karateClass, DateTime.UtcNow.Date);

        var text = enrollment.ToString();

        Assert.Contains(student.UserId.ToString(), text);
        Assert.Contains(karateClass.ClassId.ToString(), text);
    }
}
