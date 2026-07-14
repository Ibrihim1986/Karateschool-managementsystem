namespace KarateSchool.Web.Models.Entities;

/// <summary>Junction entity resolving the Student &lt;-&gt; KarateClass many-to-many relationship.</summary>
public class Enrollment
{
    public int EnrollmentId { get; private set; }

    public int StudentId { get; private set; }
    public Student Student { get; private set; } = null!;

    public int ClassId { get; private set; }
    public KarateClass KarateClass { get; private set; } = null!;

    public DateTime EnrollDate { get; private set; }

    private Enrollment()
    {
    }

    public Enrollment(Student student, KarateClass karateClass, DateTime enrollDate)
    {
        Student = student ?? throw new ArgumentNullException(nameof(student));
        KarateClass = karateClass ?? throw new ArgumentNullException(nameof(karateClass));
        StudentId = student.UserId;
        ClassId = karateClass.ClassId;

        if (enrollDate.Date > DateTime.UtcNow.Date)
            throw new ArgumentException("Enrollment date cannot be in the future.", nameof(enrollDate));
        EnrollDate = enrollDate;
    }

    public override string ToString() => $"Enrollment #{EnrollmentId}: Student {StudentId} -> Class {ClassId}";
}
