namespace KarateSchool.Web.Models.Entities;

/// <summary>
/// An instructor's belt-promotion recommendation for a student. Requires justification notes and
/// must be approved by an Administrator before it becomes final.
/// </summary>
public class BeltPromotion
{
    public int PromotionId { get; private set; }

    public int StudentId { get; private set; }
    public Student Student { get; private set; } = null!;

    public int InstructorId { get; private set; }
    public Instructor Instructor { get; private set; } = null!;

    public string NewBelt { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    public bool IsApproved { get; private set; }
    public DateTime? ApprovedDate { get; private set; }

    private BeltPromotion()
    {
    }

    public BeltPromotion(Student student, Instructor instructor, string newBelt, DateTime date, string notes)
    {
        Student = student ?? throw new ArgumentNullException(nameof(student));
        Instructor = instructor ?? throw new ArgumentNullException(nameof(instructor));
        StudentId = student.UserId;
        InstructorId = instructor.UserId;

        if (string.IsNullOrWhiteSpace(newBelt))
            throw new ArgumentException("New belt rank is required.", nameof(newBelt));
        NewBelt = newBelt.Trim();

        if (date.Date > DateTime.UtcNow.Date)
            throw new ArgumentException("Promotion date cannot be in the future.", nameof(date));
        Date = date;

        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException(
                "Belt promotion recommendations require justification notes.", nameof(notes));
        Notes = notes.Trim();

        IsApproved = false;
    }

    /// <summary>Finalizes the promotion. Only an Administrator workflow should call this.</summary>
    public void Approve()
    {
        if (IsApproved)
            throw new InvalidOperationException("Promotion has already been approved.");
        IsApproved = true;
        ApprovedDate = DateTime.UtcNow.Date;
    }

    public override string ToString() =>
        $"{(IsApproved ? "[Approved]" : "[Pending]")} Student {StudentId} -> {NewBelt} ({Date:yyyy-MM-dd})";
}
