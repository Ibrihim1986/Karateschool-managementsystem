namespace KarateSchool.Web.Models.Entities;

public class KarateClass
{
    public int ClassId { get; private set; }
    public string ClassName { get; private set; } = string.Empty;

    /// <summary>Day/time slot, e.g. "Mon/Wed 18:00-19:00". Used together with Room/Instructor to prevent double-booking.</summary>
    public string Schedule { get; private set; } = string.Empty;
    public string Room { get; private set; } = string.Empty;
    public int Capacity { get; private set; }

    public int InstructorId { get; private set; }
    public Instructor Instructor { get; private set; } = null!;

    private readonly List<Enrollment> _enrollments = new();
    private readonly List<Attendance> _attendanceRecords = new();

    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments;
    public IReadOnlyCollection<Attendance> AttendanceRecords => _attendanceRecords;

    private KarateClass()
    {
    }

    public KarateClass(string className, string schedule, string room, int capacity, Instructor instructor)
    {
        if (string.IsNullOrWhiteSpace(className))
            throw new ArgumentException("Class name is required.", nameof(className));
        if (string.IsNullOrWhiteSpace(schedule))
            throw new ArgumentException("Schedule is required.", nameof(schedule));
        if (string.IsNullOrWhiteSpace(room))
            throw new ArgumentException("Room is required.", nameof(room));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));

        ClassName = className.Trim();
        Schedule = schedule.Trim();
        Room = room.Trim();
        Capacity = capacity;

        Instructor = instructor ?? throw new ArgumentNullException(nameof(instructor));
        InstructorId = instructor.UserId;
    }

    public bool HasAvailableCapacity => _enrollments.Count < Capacity;

    public void Enroll(Enrollment enrollment)
    {
        if (enrollment is null)
            throw new ArgumentNullException(nameof(enrollment));
        if (!HasAvailableCapacity)
            throw new InvalidOperationException($"Class '{ClassName}' is at capacity ({Capacity}).");
        if (_enrollments.Any(e => e.StudentId == enrollment.StudentId))
            throw new InvalidOperationException("Student is already enrolled in this class.");
        _enrollments.Add(enrollment);
    }

    public override string ToString() => $"{ClassName} ({Schedule}, Room {Room}) - Capacity {Capacity}";
}
