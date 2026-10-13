namespace GameStore.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Genre { get; set; } = "";
    public string Platform { get; set; } = "";
    public decimal Price { get; set; }
    public string Description { get; set; } = "";
    public string ImageFile { get; set; } = "";
}
