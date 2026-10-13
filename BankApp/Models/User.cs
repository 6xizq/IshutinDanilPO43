namespace BankApp.Models;

public class User
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal Balance { get; set; }
    public List<Credit> Credits { get; set; } = new();
}
