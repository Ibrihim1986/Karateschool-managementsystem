using KarateSchool.Web.Services.Factories;
using Xunit;

namespace KarateSchool.Tests.Services;

public class UserFactoryTests
{
    [Fact]
    public void CreateStudent_ReturnsStudentWithStudentRole()
    {
        var student = UserFactory.CreateStudent(
            "Fac Student", "fac-student@example.com", "hash", "555-0000",
            new DateTime(1995, 1, 1), "White Belt", DateTime.UtcNow.Date, null);

        Assert.Equal("Student", student.Role);
    }

    [Fact]
    public void CreateInstructor_ReturnsInstructorWithInstructorRole()
    {
        var instructor = UserFactory.CreateInstructor(
            "Fac Instructor", "fac-instructor@example.com", "hash", "555-0000",
            "Judo", "3rd Dan", new DateTime(2019, 1, 1));

        Assert.Equal("Instructor", instructor.Role);
    }

    [Fact]
    public void CreateAdministrator_ReturnsAdministratorWithAdministratorRole()
    {
        var admin = UserFactory.CreateAdministrator(
            "Fac Admin", "fac-admin@example.com", "hash", "555-0000", "Full");

        Assert.Equal("Administrator", admin.Role);
    }
}
