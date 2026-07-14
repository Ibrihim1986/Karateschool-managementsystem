using System.ComponentModel.DataAnnotations.Schema;

namespace KarateSchool.Web.Models.Entities;

public class Instructor : User
{
    public string Specialty { get; private set; } = string.Empty;
    public string Certification { get; private set; } = string.Empty;
    public DateTime HireDate { get; private set; }

    /// <summary>Alias for UserId to match the UML attribute name; same physical key (TPT).</summary>
    [NotMapped]
    public int InstructorId => UserId;

    private readonly List<KarateClass> _classesTaught = new();
    private readonly List<BeltPromotion> _beltPromotionsRecommended = new();

    public IReadOnlyCollection<KarateClass> ClassesTaught => _classesTaught;
    public IReadOnlyCollection<BeltPromotion> BeltPromotionsRecommended => _beltPromotionsRecommended;

    private Instructor()
    {
    }

    public Instructor(
        string fullName,
        string email,
        string passwordHash,
        string phone,
        string specialty,
        string certification,
        DateTime hireDate)
        : base(fullName, email, passwordHash, phone, "Instructor")
    {
        SetSpecialty(specialty);
        SetCertification(certification);

        if (hireDate.Date > DateTime.UtcNow.Date)
            throw new ArgumentException("Hire date cannot be in the future.", nameof(hireDate));
        HireDate = hireDate;
    }

    public void SetSpecialty(string specialty)
    {
        if (string.IsNullOrWhiteSpace(specialty))
            throw new ArgumentException("Specialty is required.", nameof(specialty));
        Specialty = specialty.Trim();
    }

    public void SetCertification(string certification)
    {
        if (string.IsNullOrWhiteSpace(certification))
            throw new ArgumentException("Certification is required.", nameof(certification));
        Certification = certification.Trim();
    }

    /// <summary>An instructor may only act on students/classes they are assigned to.</summary>
    public bool IsAssignedTo(KarateClass karateClass)
    {
        if (karateClass is null)
            throw new ArgumentNullException(nameof(karateClass));
        return karateClass.InstructorId == UserId;
    }

    public override string ToString() =>
        $"{base.ToString()} | Specialty: {Specialty} | Certified: {Certification}";
}
