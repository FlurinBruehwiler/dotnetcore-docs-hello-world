namespace OnlineShop.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Images { get; set; } = null!;
    public int Amount { get; set; }
    public int Price { get; set; }
    public List<Order> Orders { get; set; }
}