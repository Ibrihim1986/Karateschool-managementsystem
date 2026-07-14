using KarateSchool.Web.Data;
using KarateSchool.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KarateSchool.Tests.Entities;

public class StudentTests
{
    private static DateTime AgeYears(int years) => DateTime.UtcNow.Date.AddYears(-years).AddDays(-1);

    [Fact]
    public void Constructor_MinorWithoutEmergencyContact_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new Student(
                "Minor Student", "minor@example.com", "hash", "555-0000",
                AgeYears(10), "White Belt", DateTime.UtcNow.Date, emergencyContact: null));

        Assert.Contains("under 18", ex.Message);
    }

    [Fact]
    public void Constructor_MinorWithEmergencyContact_Succeeds()
    {
        var student = new Student(
            "Minor Student", "minor2@example.com", "hash", "555-0000",
            AgeYears(10), "White Belt", DateTime.UtcNow.Date, emergencyContact: "Parent, 555-1111");

        Assert.True(student.IsMinor);
        Assert.Equal("Parent, 555-1111", student.EmergencyContact);
    }

    [Fact]
    public void Constructor_AdultWithoutEmergencyContact_Succeeds()
    {
        var student = new Student(
            "Adult Student", "adult@example.com", "hash", "555-0000",
            AgeYears(30), "White Belt", DateTime.UtcNow.Date, emergencyContact: null);

        Assert.False(student.IsMinor);
    }

    [Fact]
    public void Constructor_FutureDateOfBirth_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Student(
                "Future Student", "future@example.com", "hash", "555-0000",
                DateTime.UtcNow.Date.AddDays(1), "White Belt", DateTime.UtcNow.Date, "Guardian"));
    }

    [Theory]
    [InlineData(17, true)]
    [InlineData(18, false)]
    [InlineData(25, false)]
    public void IsMinor_ReflectsAgeThreshold(int age, bool expectedIsMinor)
    {
        var student = new Student(
            "Student", $"age{age}@example.com", "hash", "555-0000",
            AgeYears(age), "White Belt", DateTime.UtcNow.Date, "Guardian");

        Assert.Equal(expectedIsMinor, student.IsMinor);
    }

    [Fact]
    public void GetAttendanceRate_NoRecords_ReturnsZero()
    {
        var student = CreateAdultStudent();
        Assert.Equal(0, student.GetAttendanceRate());
    }

    [Fact]
    public void GetAttendanceRate_ComputesPresentRatio()
    {
        var student = CreateAdultStudent();
        var instructor = new Instructor(
            "Instr", "instr-att@example.com", "hash", "555-0000", "Karate", "Black Belt", new DateTime(2020, 1, 1));
        var karateClass = new KarateClass("Test Class", "Mon 10:00", "Room 1", 10, instructor);

        student.RecordAttendance(new Attendance(student, karateClass, DateTime.UtcNow.Date.AddDays(-1), "Present"));
        student.RecordAttendance(new Attendance(student, karateClass, DateTime.UtcNow.Date.AddDays(-2), "Absent"));

        Assert.Equal(0.5, student.GetAttendanceRate());
    }

    [Fact]
    public void MakePayment_TracksTotalPaidForCompletedPaymentsOnly()
    {
        var student = CreateAdultStudent();
        student.MakePayment(new Payment(student, 50m, DateTime.UtcNow.Date, "Cash", "Completed"));
        student.MakePayment(new Payment(student, 30m, DateTime.UtcNow.Date, "Card", "Pending"));

        Assert.Equal(50m, student.GetTotalPaid());
        Assert.Equal(50m, student.GetOutstandingBalance(100m));
    }

    private static Student CreateAdultStudent() =>
        new(
            "Adult Student", $"adult-{Guid.NewGuid():N}@example.com", "hash", "555-0000",
            AgeYears(30), "White Belt", DateTime.UtcNow.Date, emergencyContact: null);

    [Fact]
    public void Constructor_EmptyBeltRank_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Student(
                "Student", "beltrank@example.com", "hash", "555-0000",
                AgeYears(30), "", DateTime.UtcNow.Date, null));
    }

    [Fact]
    public void RecordAttendance_Null_ThrowsArgumentNullException()
    {
        var student = CreateAdultStudent();
        Assert.Throws<ArgumentNullException>(() => student.RecordAttendance(null!));
    }

    [Fact]
    public void RecordAttendance_ForDifferentStudent_ThrowsInvalidOperationException()
    {
        // Both students need real, distinct auto-generated IDs — unsaved entities all default
        // to UserId 0, which would make any two in-memory students compare as "the same" student.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new ApplicationDbContext(options);

        var student = CreateAdultStudent();
        var otherStudent = CreateAdultStudent();
        var instructor = new Instructor(
            "Instr", "instr-mismatch@example.com", "hash", "555-0000", "Karate", "Black Belt", new DateTime(2020, 1, 1));
        context.Students.AddRange(student, otherStudent);
        context.Instructors.Add(instructor);
        context.SaveChanges();

        var karateClass = new KarateClass("Class", "Mon 10:00", "Room 1", 10, instructor);
        var attendance = new Attendance(otherStudent, karateClass, DateTime.UtcNow.Date, "Present");

        Assert.Throws<InvalidOperationException>(() => student.RecordAttendance(attendance));
    }

    [Fact]
    public void MakePayment_Null_ThrowsArgumentNullException()
    {
        var student = CreateAdultStudent();
        Assert.Throws<ArgumentNullException>(() => student.MakePayment(null!));
    }

    [Fact]
    public void MakePayment_ForDifferentStudent_ThrowsInvalidOperationException()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new ApplicationDbContext(options);

        var student = CreateAdultStudent();
        var otherStudent = CreateAdultStudent();
        context.Students.AddRange(student, otherStudent);
        context.SaveChanges();

        var payment = new Payment(otherStudent, 50m, DateTime.UtcNow.Date, "Cash", "Completed");

        Assert.Throws<InvalidOperationException>(() => student.MakePayment(payment));
    }

    [Fact]
    public void ToString_IncludesBeltRankAndEnrollmentDate()
    {
        var student = CreateAdultStudent();
        var text = student.ToString();

        Assert.Contains("White Belt", text);
    }
}
