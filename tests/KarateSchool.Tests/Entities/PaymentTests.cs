using KarateSchool.Web.Models.Entities;
using Xunit;

namespace KarateSchool.Tests.Entities;

public class PaymentTests
{
    private static Student CreateStudent() =>
        new("Payment Student", "payment-student@example.com", "hash", "555-0000",
            new DateTime(1990, 1, 1), "White Belt", DateTime.UtcNow.Date, null);

    [Fact]
    public void Constructor_NullStudent_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Payment(null!, 50m, DateTime.UtcNow.Date, "Cash", "Completed"));
    }

    [Fact]
    public void Constructor_NonPositiveAmount_ThrowsArgumentException()
    {
        var student = CreateStudent();
        Assert.Throws<ArgumentException>(() =>
            new Payment(student, 0m, DateTime.UtcNow.Date, "Cash", "Completed"));
    }

    [Fact]
    public void Constructor_InvalidMethod_ThrowsArgumentException()
    {
        var student = CreateStudent();
        Assert.Throws<ArgumentException>(() =>
            new Payment(student, 50m, DateTime.UtcNow.Date, "Bitcoin", "Completed"));
    }

    [Fact]
    public void Constructor_EmptyMethod_ThrowsArgumentException()
    {
        var student = CreateStudent();
        Assert.Throws<ArgumentException>(() =>
            new Payment(student, 50m, DateTime.UtcNow.Date, "", "Completed"));
    }

    [Fact]
    public void Constructor_InvalidStatus_ThrowsArgumentException()
    {
        var student = CreateStudent();
        Assert.Throws<ArgumentException>(() =>
            new Payment(student, 50m, DateTime.UtcNow.Date, "Cash", "Unknown"));
    }

    [Fact]
    public void Constructor_EmptyStatus_ThrowsArgumentException()
    {
        var student = CreateStudent();
        Assert.Throws<ArgumentException>(() =>
            new Payment(student, 50m, DateTime.UtcNow.Date, "Cash", ""));
    }

    [Fact]
    public void ToString_IncludesAmountAndMethod()
    {
        var student = CreateStudent();
        var payment = new Payment(student, 50m, DateTime.UtcNow.Date, "Cash", "Completed");

        var text = payment.ToString();

        Assert.Contains("Cash", text);
        Assert.Contains("Completed", text);
    }
}
