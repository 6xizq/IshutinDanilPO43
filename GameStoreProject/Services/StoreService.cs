using System.Text.Json;
using GameStore.Models;

namespace GameStore.Services;

public static class StoreService
{
    public static readonly string Root = AppContext.BaseDirectory;
    public static readonly string DataDir = Path.Combine(Root, "Data");
    public static readonly string ReceiptsDir = Path.Combine(Root, "Receipts");
    public static readonly string AssetsDir = Path.Combine(Root, "Assets");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public static List<User> Users { get; private set; } = new();
    public static List<Product> Products { get; private set; } = new();
    public static List<CartItem> Cart { get; private set; } = new();
    public static User? CurrentUser { get; private set; }

    private static string UsersFile => Path.Combine(DataDir, "users.json");
    private static string ProductsFile => Path.Combine(DataDir, "products.json");
    private static string CartFile(string login) => Path.Combine(DataDir, $"cart_{Sanitize(login)}.json");

    public static void Initialize()
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(ReceiptsDir);
        Directory.CreateDirectory(AssetsDir);
        if (File.Exists(UsersFile)) Users = Load<List<User>>(UsersFile) ?? new();
        if (Users.Count == 0)
        {
            Users.Add(new User { Login = "admin", Password = "admin", Name = "Администратор", Email = "admin@gamestore.local" });
            Save(UsersFile, Users);
        }
        if (File.Exists(ProductsFile)) Products = Load<List<Product>>(ProductsFile) ?? new();
        if (Products.Count == 0)
        {
            Products = new List<Product>
            {
                new() { Id=1, Name="Cyberpunk 2077", Genre="RPG", Platform="PC", Price=14990, ImageFile="cyberpunk.png", Description="Футуристическая ролевая игра с открытым миром Найт-Сити." },
                new() { Id=2, Name="Minecraft", Genre="Sandbox", Platform="PC", Price=8990, ImageFile="minecraft.png", Description="Исследование, строительство и приключения в процедурном мире." },
                new() { Id=3, Name="EA Sports FC 26", Genre="Sports", Platform="PC", Price=17990, ImageFile="fc26.png", Description="Футбольный симулятор с клубами и режимами карьеры." },
                new() { Id=4, Name="Baldur's Gate 3", Genre="RPG", Platform="PC", Price=22990, ImageFile="bg3.png", Description="Большое сюжетное RPG-приключение с пошаговыми боями." },
                new() { Id=5, Name="Forza Horizon 5", Genre="Racing", Platform="PC", Price=12990, ImageFile="forza.png", Description="Аркадные гонки по открытому миру Мексики." },
                new() { Id=6, Name="Hades II", Genre="Action", Platform="PC", Price=10990, ImageFile="hades2.png", Description="Динамичный roguelike-экшен с мифологическим сеттингом." }
            };
            Save(ProductsFile, Products);
        }
    }

    public static bool Login(string login, string password)
    {
        var user = Users.FirstOrDefault(u => u.Login.Equals(login.Trim(), StringComparison.OrdinalIgnoreCase) && u.Password == password);
        if (user == null) return false;
        CurrentUser = user;
        Cart = Load<List<CartItem>>(CartFile(user.Login)) ?? new();
        return true;
    }

    public static bool Register(User user)
    {
        if (Users.Any(u => u.Login.Equals(user.Login.Trim(), StringComparison.OrdinalIgnoreCase))) return false;
        Users.Add(user);
        Save(UsersFile, Users);
        return true;
    }

    public static bool ChangePassword(string login, string oldPassword, string newPassword)
    {
        var user = Users.FirstOrDefault(u => u.Login.Equals(login.Trim(), StringComparison.OrdinalIgnoreCase) && u.Password == oldPassword);
        if (user == null) return false;
        user.Password = newPassword;
        Save(UsersFile, Users);
        return true;
    }

    public static void AddToCart(Product product)
    {
        var item = Cart.FirstOrDefault(x => x.Product.Id == product.Id);
        if (item == null) Cart.Add(new CartItem { Product = product, Quantity = 1 });
        else item.Quantity++;
        SaveCart();
    }

    public static void RemoveFromCart(int productId)
    {
        var item = Cart.FirstOrDefault(x => x.Product.Id == productId);
        if (item == null) return;
        item.Quantity--;
        if (item.Quantity <= 0) Cart.Remove(item);
        SaveCart();
    }

    public static void ClearCart() { Cart.Clear(); SaveCart(); }
    public static decimal CartTotal() => Cart.Sum(x => x.Total);
    public static void SaveCart() { if (CurrentUser != null) Save(CartFile(CurrentUser.Login), Cart); }

    public static string CreateReceipt()
    {
        Directory.CreateDirectory(ReceiptsDir);
        var now = DateTime.Now;
        var file = Path.Combine(ReceiptsDir, $"receipt_{now:yyyyMMdd_HHmmss_fff}.txt");
        var lines = new List<string>
        {
            "========================================",
            "              GAME STORE",
            "           ЧЕК О ПОКУПКЕ",
            "========================================",
            $"Покупатель: {CurrentUser?.Name}",
            $"Дата и время: {now:dd.MM.yyyy HH:mm:ss}",
            "----------------------------------------"
        };
        foreach (var item in Cart) lines.Add($"{item.Product.Name} x{item.Quantity} — {item.Total:N0} тг");
        lines.Add("----------------------------------------");
        lines.Add($"ИТОГО: {CartTotal():N0} тг");
        lines.Add("========================================");
        File.WriteAllLines(file, lines);
        return file;
    }

    private static string Sanitize(string value) => string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
    private static T? Load<T>(string file) { try { return JsonSerializer.Deserialize<T>(File.ReadAllText(file), JsonOptions); } catch { return default; } }
    private static void Save<T>(string file, T data) => File.WriteAllText(file, JsonSerializer.Serialize(data, JsonOptions));
}
