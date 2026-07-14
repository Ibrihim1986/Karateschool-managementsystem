namespace KarateSchool.Web.Models.Interfaces;

public interface IPayable
{
    IReadOnlyCollection<Entities.Payment> Payments { get; }

    void MakePayment(Entities.Payment payment);

    decimal GetTotalPaid();

    decimal GetOutstandingBalance(decimal totalDue);
}
