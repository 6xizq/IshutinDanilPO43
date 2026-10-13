namespace GameStore.Models;

public class CartData
{
    public string Login { get; set; } = "";
    public List<CartItem> Items { get; set; } = new();
}
