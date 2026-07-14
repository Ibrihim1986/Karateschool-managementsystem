using KarateSchool.Web.Models.Entities;

namespace KarateSchool.Web.Services.Factories;

/// <summary>Factory pattern: centralizes construction of the concrete User subtypes.</summary>
public static class UserFactory
{
    public static Student CreateStudent(
        string fullName,
        string email,
        string passwordHash,
        string phone,
        DateTime dateOfBirth,
        string beltRank,
        DateTime enrollmentDate,
        string? emergencyContact) =>
        new(fullName, email, passwordHash, phone, dateOfBirth, beltRank, enrollmentDate, emergencyContact);

    public static Instructor CreateInstructor(
        string fullName,
        string email,
        string passwordHash,
        string phone,
        string specialty,
        string certification,
        DateTime hireDate) =>
        new(fullName, email, passwordHash, phone, specialty, certification, hireDate);

    public static Administrator CreateAdministrator(
        string fullName,
        string email,
        string passwordHash,
        string phone,
        string accessLevel) =>
        new(fullName, email, passwordHash, phone, accessLevel);
}
