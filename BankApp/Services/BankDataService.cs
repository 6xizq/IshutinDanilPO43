using System.Text.Json;
using BankApp.Models;

namespace BankApp.Services;

public static class BankDataService
{
    private static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BankApp");
    private static readonly string FilePath = Path.Combine(Folder, "bank-data.json");

    public static List<User> Users { get; private set; } = new();
    public static User? CurrentUser { get; set; }

    public static void Initialize()
    {
        Directory.CreateDirectory(Folder);
        if (File.Exists(FilePath))
        {
            try { Users = JsonSerializer.Deserialize<List<User>>(File.ReadAllText(FilePath)) ?? new(); }
            catch { Users = new(); }
        }
        if (Users.Count == 0)
        {
            Users.Add(new User {
                Login = "admin", Password = "1234", FullName = "Тестовый пользователь",
                Phone = "+7 700 000 00 00", Email = "admin@bank.local", Balance = 125000
            });
            Save();
        }
    }

    public static void Save()
    {
        Directory.CreateDirectory(Folder);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(Users,
            new JsonSerializerOptions { WriteIndented = true }));
    }

    public static User? Authenticate(string login, string password) =>
        Users.FirstOrDefault(x => x.Login.Equals(login, StringComparison.OrdinalIgnoreCase)
                               && x.Password == password);

    public static bool LoginExists(string login) =>
        Users.Any(x => x.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
}
