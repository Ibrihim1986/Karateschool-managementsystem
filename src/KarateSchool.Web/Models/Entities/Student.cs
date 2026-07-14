using System.ComponentModel.DataAnnotations.Schema;
using KarateSchool.Web.Models.Interfaces;

namespace KarateSchool.Web.Models.Entities;

public class Student : User, IAttendanceTrackable, IPayable
{
    public const int MinorAge = 18;

    public DateTime DateOfBirth { get; private set; }
    public string BeltRank { get; private set; } = "White Belt";
    public DateTime EnrollmentDate { get; private set; }
    public string? EmergencyContact { get; private set; }

    /// <summary>Alias for UserId to match the UML attribute name; same physical key (TPT).</summary>
    [NotMapped]
    public int StudentId => UserId;

    private readonly List<Enrollment> _enrollments = new();
    private readonly List<Attendance> _attendanceRecords = new();
    private readonly List<BeltPromotion> _beltPromotions = new();
    private readonly List<Payment> _payments = new();

    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments;
    public IReadOnlyCollection<Attendance> AttendanceRecords => _attendanceRecords;
    public IReadOnlyCollection<BeltPromotion> BeltPromotions => _beltPromotions;
    public IReadOnlyCollection<Payment> Payments => _payments;

    private Student()
    {
    }

    public Student(
        string fullName,
        string email,
        string passwordHash,
        string phone,
        DateTime dateOfBirth,
        string beltRank,
        DateTime enrollmentDate,
        string? emergencyContact)
        : base(fullName, email, passwordHash, phone, "Student")
    {
        SetDateOfBirth(dateOfBirth, emergencyContact);
        SetBeltRank(beltRank);
        EnrollmentDate = enrollmentDate;
        EmergencyContact = emergencyContact;
    }

    public int Age => CalculateAge(DateOfBirth);

    public bool IsMinor => Age < MinorAge;

    public static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;
        return age;
    }

    private void SetDateOfBirth(DateTime dateOfBirth, string? emergencyContact)
    {
        if (dateOfBirth.Date >= DateTime.UtcNow.Date)
            throw new ArgumentException("Date of birth must be in the past.", nameof(dateOfBirth));

        if (CalculateAge(dateOfBirth) < MinorAge && string.IsNullOrWhiteSpace(emergencyContact))
            throw new InvalidOperationException(
                "Students under 18 require a guardian/emergency contact on file.");

        DateOfBirth = dateOfBirth;
    }

    public void SetBeltRank(string beltRank)
    {
        if (string.IsNullOrWhiteSpace(beltRank))
            throw new ArgumentException("Belt rank is required.", nameof(beltRank));
        BeltRank = beltRank.Trim();
    }

    public void RecordAttendance(Attendance attendance)
    {
        if (attendance is null)
            throw new ArgumentNullException(nameof(attendance));
        if (attendance.StudentId != UserId)
            throw new InvalidOperationException("Attendance record does not belong to this student.");
        _attendanceRecords.Add(attendance);
    }

    public double GetAttendanceRate()
    {
        if (_attendanceRecords.Count == 0)
            return 0d;
        var present = _attendanceRecords.Count(a => a.Status == "Present");
        return (double)present / _attendanceRecords.Count;
    }

    public void MakePayment(Payment payment)
    {
        if (payment is null)
            throw new ArgumentNullException(nameof(payment));
        if (payment.StudentId != UserId)
            throw new InvalidOperationException("Payment does not belong to this student.");
        _payments.Add(payment);
    }

    public decimal GetTotalPaid() => _payments
        .Where(p => p.Status == "Completed")
        .Sum(p => p.Amount);

    public decimal GetOutstandingBalance(decimal totalDue) => Math.Max(0, totalDue - GetTotalPaid());

    public override string ToString() =>
        $"{base.ToString()} | Belt: {BeltRank} | Enrolled: {EnrollmentDate:yyyy-MM-dd}";
}
