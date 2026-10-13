namespace BankApp.Models;

public class Credit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; }
    public decimal Remaining { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime NextPaymentDate { get; set; } = DateTime.Now.AddMonths(1);
    public bool IsPaid => Remaining <= 0;
}
