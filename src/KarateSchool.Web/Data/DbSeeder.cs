using KarateSchool.Web.Models.Entities;
using KarateSchool.Web.Services.Auth;
using KarateSchool.Web.Services.Factories;
using Microsoft.EntityFrameworkCore;

namespace KarateSchool.Web.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        context.Database.Migrate();

        if (context.Users.Any())
            return;

        var defaultPasswordHash = passwordHasher.HashPassword("Passw0rd!");

        var admin = UserFactory.CreateAdministrator(
            "Alice Administrator", "admin@karateschool.com", defaultPasswordHash, "555-0100", "SuperAdmin");

        var instructor1 = UserFactory.CreateInstructor(
            "Ichiro Sensei", "instructor1@karateschool.com", defaultPasswordHash, "555-0101",
            "Shotokan Karate", "5th Dan Black Belt", new DateTime(2018, 3, 1));

        var instructor2 = UserFactory.CreateInstructor(
            "Kenji Sensei", "instructor2@karateschool.com", defaultPasswordHash, "555-0102",
            "Kickboxing", "4th Dan Black Belt", new DateTime(2020, 6, 15));

        var student1 = UserFactory.CreateStudent(
            "Sam Student", "student1@karateschool.com", defaultPasswordHash, "555-0201",
            new DateTime(2010, 5, 20), "Yellow Belt", new DateTime(2023, 1, 10), "Parent: Jane Student, 555-0299");

        var student2 = UserFactory.CreateStudent(
            "Taylor Trainee", "student2@karateschool.com", defaultPasswordHash, "555-0202",
            new DateTime(2000, 8, 2), "Green Belt", new DateTime(2022, 9, 5), null);

        var student3 = UserFactory.CreateStudent(
            "Jordan Junior", "student3@karateschool.com", defaultPasswordHash, "555-0203",
            new DateTime(2012, 11, 30), "White Belt", DateTime.UtcNow.Date, "Parent: Morgan Junior, 555-0298");

        context.Administrators.Add(admin);
        context.Instructors.AddRange(instructor1, instructor2);
        context.Students.AddRange(student1, student2, student3);
        context.SaveChanges();

        var beginnersClass = new KarateClass("Beginners Karate", "Mon/Wed 17:00-18:00", "Dojo A", 15, instructor1);
        var advancedClass = new KarateClass("Advanced Karate", "Tue/Thu 18:00-19:30", "Dojo A", 12, instructor1);
        var kickboxingClass = new KarateClass("Kickboxing Basics", "Fri 17:00-18:30", "Dojo B", 10, instructor2);

        context.KarateClasses.AddRange(beginnersClass, advancedClass, kickboxingClass);
        context.SaveChanges();

        var enrollment1 = new Enrollment(student1, beginnersClass, new DateTime(2023, 1, 10));
        var enrollment2 = new Enrollment(student2, advancedClass, new DateTime(2022, 9, 5));
        var enrollment3 = new Enrollment(student3, beginnersClass, DateTime.UtcNow.Date);

        context.Enrollments.AddRange(enrollment1, enrollment2, enrollment3);

        var attendance1 = new Attendance(student1, beginnersClass, DateTime.UtcNow.Date.AddDays(-7), "Present");
        var attendance2 = new Attendance(student1, beginnersClass, DateTime.UtcNow.Date.AddDays(-2), "Absent");
        var attendance3 = new Attendance(student2, advancedClass, DateTime.UtcNow.Date.AddDays(-3), "Present");

        context.Attendances.AddRange(attendance1, attendance2, attendance3);

        var promotion1 = new BeltPromotion(
            student2, instructor1, "Blue Belt", DateTime.UtcNow.Date.AddDays(-30),
            "Demonstrated strong form and sparring discipline over the last term.");
        promotion1.Approve();

        var promotion2 = new BeltPromotion(
            student1, instructor1, "Orange Belt", DateTime.UtcNow.Date.AddDays(-1),
            "Consistent attendance and mastery of the required kata.");

        context.BeltPromotions.AddRange(promotion1, promotion2);

        var payment1 = new Payment(student1, 75.00m, DateTime.UtcNow.Date.AddDays(-10), "Card", "Completed");
        var payment2 = new Payment(student2, 90.00m, DateTime.UtcNow.Date.AddDays(-15), "Cash", "Completed");

        context.Payments.AddRange(payment1, payment2);

        var announcement = new Announcement(
            admin, "Welcome to the new term!",
            "Classes resume this week. Please check the updated schedule on the dashboard.",
            DateTime.UtcNow.Date.AddDays(-1));

        context.Announcements.Add(announcement);

        context.SaveChanges();
    }
}
