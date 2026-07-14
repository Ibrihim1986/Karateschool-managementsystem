namespace KarateSchool.Web.Models.Entities;

public class Payment
{
    public static readonly string[] ValidMethods = { "Cash", "Card", "BankTransfer", "Other" };
    public static readonly string[] ValidStatuses = { "Pending", "Completed", "Failed", "Refunded" };

    public int PaymentId { get; private set; }

    public int StudentId { get; private set; }
    public Student Student { get; private set; } = null!;

    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }
    public string Method { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;

    private Payment()
    {
    }

    public Payment(Student student, decimal amount, DateTime date, string method, string status)
    {
        Student = student ?? throw new ArgumentNullException(nameof(student));
        StudentId = student.UserId;

        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));
        Amount = amount;

        Date = date;

        if (string.IsNullOrWhiteSpace(method) || !ValidMethods.Contains(method))
            throw new ArgumentException(
                $"Method must be one of: {string.Join(", ", ValidMethods)}.", nameof(method));
        Method = method;

        SetStatus(status);
    }

    public void SetStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !ValidStatuses.Contains(status))
            throw new ArgumentException(
                $"Status must be one of: {string.Join(", ", ValidStatuses)}.", nameof(status));
        Status = status;
    }

    public override string ToString() => $"{Date:yyyy-MM-dd} - {Amount:C} via {Method} [{Status}]";
}
