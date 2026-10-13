using System;
using System.Collections.Generic;

class SmartFridge
{
    private List<string> _products;
    private int _maxCapacity;

    public SmartFridge(int maxCapacity)
    {
        _maxCapacity = maxCapacity;
        _products = new List<string>();
    }

    public void AddProduct(string product)
    {
        if (_products.Count >= _maxCapacity)
        {
            Console.WriteLine("Холодильник полон!");
            return;
        }
        _products.Add(product);
        Console.WriteLine($"Добавлен продукт: {product}");
    }

    public void RemoveProduct(string product)
    {
        if (_products.Contains(product))
        {
            _products.Remove(product);
            Console.WriteLine($"Удален продукт: {product}");
        }
        else
        {
            Console.WriteLine("Такого продукта нет!");
        }
    }

    public IReadOnlyList<string> GetProducts()
    {
        return _products.AsReadOnly();
    }
}
class Program
{
    static void Main()
    {
        SmartFridge fridge = new SmartFridge(3);

        Console.WriteLine("--- Добавление 4 продуктов ---");
        fridge.AddProduct("Пицца");
        fridge.AddProduct("Сок");
        fridge.AddProduct("Яблоки");
        fridge.AddProduct("Йогурт");

        Console.WriteLine("\n--- Удаление продуктов ---");
        fridge.RemoveProduct("Сок");
        fridge.RemoveProduct("Мороженое");

        Console.WriteLine("\n--- Содержимое холодильника ---");
        var currentProducts = fridge.GetProducts();
        foreach (var item in currentProducts)
        {
            Console.WriteLine($"- {item}");
        }
    }
}