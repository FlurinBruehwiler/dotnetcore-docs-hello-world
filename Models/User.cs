namespace OnlineShop.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public required byte[] PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }
    public bool IsAdmin { get; set; }
    public List<Order> Orders { get; set; } = null!;
}