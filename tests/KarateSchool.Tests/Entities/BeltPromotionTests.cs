using KarateSchool.Web.Models.Entities;
using Xunit;

namespace KarateSchool.Tests.Entities;

public class BeltPromotionTests
{
    private static (Student student, Instructor instructor) CreateFixtures()
    {
        var instructor = new Instructor(
            "Instr", "instr-bp@example.com", "hash", "555-0000", "Karate", "Black Belt", new DateTime(2020, 1, 1));
        var student = new Student(
            "Student", "student-bp@example.com", "hash", "555-0000",
            new DateTime(1990, 1, 1), "White Belt", DateTime.UtcNow.Date, null);
        return (student, instructor);
    }

    [Fact]
    public void Constructor_EmptyNotes_ThrowsArgumentException()
    {
        var (student, instructor) = CreateFixtures();

        var ex = Assert.Throws<ArgumentException>(() =>
            new BeltPromotion(student, instructor, "Blue Belt", DateTime.UtcNow.Date, notes: ""));

        Assert.Contains("justification notes", ex.Message);
    }

    [Fact]
    public void Constructor_Valid_IsNotApprovedByDefault()
    {
        var (student, instructor) = CreateFixtures();
        var promotion = new BeltPromotion(
            student, instructor, "Blue Belt", DateTime.UtcNow.Date, "Great progress this term.");

        Assert.False(promotion.IsApproved);
        Assert.Null(promotion.ApprovedDate);
    }

    [Fact]
    public void Approve_SetsApprovedFlagAndDate()
    {
        var (student, instructor) = CreateFixtures();
        var promotion = new BeltPromotion(
            student, instructor, "Blue Belt", DateTime.UtcNow.Date, "Great progress this term.");

        promotion.Approve();

        Assert.True(promotion.IsApproved);
        Assert.NotNull(promotion.ApprovedDate);
    }

    [Fact]
    public void Approve_AlreadyApproved_ThrowsInvalidOperationException()
    {
        var (student, instructor) = CreateFixtures();
        var promotion = new BeltPromotion(
            student, instructor, "Blue Belt", DateTime.UtcNow.Date, "Great progress this term.");
        promotion.Approve();

        Assert.Throws<InvalidOperationException>(() => promotion.Approve());
    }

    [Fact]
    public void Constructor_NullStudent_ThrowsArgumentNullException()
    {
        var (_, instructor) = CreateFixtures();
        Assert.Throws<ArgumentNullException>(() =>
            new BeltPromotion(null!, instructor, "Blue Belt", DateTime.UtcNow.Date, "Notes"));
    }

    [Fact]
    public void Constructor_NullInstructor_ThrowsArgumentNullException()
    {
        var (student, _) = CreateFixtures();
        Assert.Throws<ArgumentNullException>(() =>
            new BeltPromotion(student, null!, "Blue Belt", DateTime.UtcNow.Date, "Notes"));
    }

    [Fact]
    public void Constructor_EmptyNewBelt_ThrowsArgumentException()
    {
        var (student, instructor) = CreateFixtures();
        Assert.Throws<ArgumentException>(() =>
            new BeltPromotion(student, instructor, "", DateTime.UtcNow.Date, "Notes"));
    }

    [Fact]
    public void Constructor_FutureDate_ThrowsArgumentException()
    {
        var (student, instructor) = CreateFixtures();
        Assert.Throws<ArgumentException>(() =>
            new BeltPromotion(student, instructor, "Blue Belt", DateTime.UtcNow.Date.AddDays(1), "Notes"));
    }

    [Fact]
    public void ToString_ReflectsApprovalState()
    {
        var (student, instructor) = CreateFixtures();
        var promotion = new BeltPromotion(
            student, instructor, "Blue Belt", DateTime.UtcNow.Date, "Great progress this term.");

        Assert.Contains("Pending", promotion.ToString());

        promotion.Approve();

        Assert.Contains("Approved", promotion.ToString());
    }
}
