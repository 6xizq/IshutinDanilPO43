namespace BankApp.Models;

public class Transaction
{
    public DateTime Date { get; set; } = DateTime.Now;
    public string Type { get; set; } = "";
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
}
