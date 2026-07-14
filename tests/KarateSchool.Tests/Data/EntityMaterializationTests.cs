using KarateSchool.Web.Data;
using KarateSchool.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KarateSchool.Tests.Data;

/// <summary>
/// EF Core only invokes an entity's private parameterless constructor when materializing a row
/// from a query into a *new* tracked instance. Every other test in this suite either constructs
/// entities directly via the public constructor or re-reads an already-tracked instance from the
/// same DbContext (a no-op for the change tracker), so those private constructors — and the
/// simple navigation-collection/ID getters only exercised via a real query — never actually run.
/// These tests force a genuine re-materialization from a second, untracked DbContext instance
/// pointed at the same in-memory database.
/// </summary>
public class EntityMaterializationTests
{
    private static DbContextOptions<ApplicationDbContext> CreateOptions(string dbName) =>
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(dbName).Options;

    [Fact]
    public async Task AllEntities_ReMaterializeCleanlyFromFreshContext()
    {
        var dbName = Guid.NewGuid().ToString();

        int studentId, instructorId, adminId, classId;
        using (var seedContext = new ApplicationDbContext(CreateOptions(dbName)))
        {
            var admin = new Administrator("Mat Admin", "mat-admin@example.com", "hash", "555-0000", "Full");
            var instructor = new Instructor(
                "Mat Instr", "mat-instr@example.com", "hash", "555-0000",
                "Karate", "Black Belt", new DateTime(2020, 1, 1));
            var student = new Student(
                "Mat Student", "mat-student@example.com", "hash", "555-0000",
                new DateTime(1990, 1, 1), "White Belt", DateTime.UtcNow.Date, null);
            seedContext.Administrators.Add(admin);
            seedContext.Instructors.Add(instructor);
            seedContext.Students.Add(student);
            await seedContext.SaveChangesAsync();

            var karateClass = new KarateClass("Mat Class", "Mon 10:00", "Room 1", 10, instructor);
            seedContext.KarateClasses.Add(karateClass);
            await seedContext.SaveChangesAsync();

            seedContext.Enrollments.Add(new Enrollment(student, karateClass, DateTime.UtcNow.Date));
            seedContext.Attendances.Add(new Attendance(student, karateClass, DateTime.UtcNow.Date, "Present"));
            seedContext.BeltPromotions.Add(new BeltPromotion(
                student, instructor, "Yellow Belt", DateTime.UtcNow.Date, "Solid fundamentals."));
            seedContext.Payments.Add(new Payment(student, 25m, DateTime.UtcNow.Date, "Cash", "Completed"));
            seedContext.Announcements.Add(new Announcement(admin, "Title", "Content", DateTime.UtcNow.Date));
            await seedContext.SaveChangesAsync();

            studentId = student.UserId;
            instructorId = instructor.UserId;
            adminId = admin.UserId;
            classId = karateClass.ClassId;
        }

        using var freshContext = new ApplicationDbContext(CreateOptions(dbName));

        var reloadedStudent = await freshContext.Students.AsNoTracking()
            .SingleAsync(s => s.UserId == studentId);
        Assert.Empty(reloadedStudent.Enrollments);
        Assert.Empty(reloadedStudent.AttendanceRecords);
        Assert.Empty(reloadedStudent.BeltPromotions);
        Assert.Empty(reloadedStudent.Payments);
        Assert.Equal(studentId, reloadedStudent.StudentId);

        var reloadedInstructor = await freshContext.Instructors.AsNoTracking()
            .SingleAsync(i => i.UserId == instructorId);
        Assert.Empty(reloadedInstructor.ClassesTaught);
        Assert.Empty(reloadedInstructor.BeltPromotionsRecommended);

        var reloadedClass = await freshContext.KarateClasses.AsNoTracking()
            .SingleAsync(c => c.ClassId == classId);
        Assert.Empty(reloadedClass.Enrollments);
        Assert.Empty(reloadedClass.AttendanceRecords);

        var reloadedAnnouncement = await freshContext.Announcements.AsNoTracking().SingleAsync();
        Assert.True(reloadedAnnouncement.AnnouncementId > 0);

        var reloadedAttendance = await freshContext.Attendances.AsNoTracking().SingleAsync();
        Assert.True(reloadedAttendance.AttendanceId > 0);

        var reloadedPromotion = await freshContext.BeltPromotions.AsNoTracking().SingleAsync();
        Assert.True(reloadedPromotion.PromotionId > 0);

        var reloadedPayment = await freshContext.Payments.AsNoTracking().SingleAsync();
        Assert.True(reloadedPayment.PaymentId > 0);

        var reloadedEnrollment = await freshContext.Enrollments.AsNoTracking().SingleAsync();
        Assert.True(reloadedEnrollment.EnrollmentId > 0);
    }
}
