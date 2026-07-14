namespace KarateSchool.Web.Models.Entities;

public class Attendance
{
    public static readonly string[] ValidStatuses = { "Present", "Absent", "Late", "Excused" };

    public int AttendanceId { get; private set; }

    public int StudentId { get; private set; }
    public Student Student { get; private set; } = null!;

    public int ClassId { get; private set; }
    public KarateClass KarateClass { get; private set; } = null!;

    public DateTime Date { get; private set; }
    public string Status { get; private set; } = string.Empty;

    private Attendance()
    {
    }

    public Attendance(Student student, KarateClass karateClass, DateTime date, string status)
    {
        Student = student ?? throw new ArgumentNullException(nameof(student));
        KarateClass = karateClass ?? throw new ArgumentNullException(nameof(karateClass));
        StudentId = student.UserId;
        ClassId = karateClass.ClassId;

        if (date.Date > DateTime.UtcNow.Date)
            throw new ArgumentException("Attendance cannot be recorded for a future date.", nameof(date));
        Date = date;

        SetStatus(status);
    }

    public void SetStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !ValidStatuses.Contains(status))
            throw new ArgumentException(
                $"Status must be one of: {string.Join(", ", ValidStatuses)}.", nameof(status));
        Status = status;
    }

    public override string ToString() => $"{Date:yyyy-MM-dd} - Student {StudentId}: {Status}";
}
