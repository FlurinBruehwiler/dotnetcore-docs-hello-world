namespace OnlineShop.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public bool IsCompleted { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
    public List<Product> Products { get; set; }
}