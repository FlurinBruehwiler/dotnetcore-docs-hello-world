using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;

namespace OnlineShop.Data;

public class MySqlContext : DbContext, IDbContext
{
    public MySqlContext(DbContextOptions<MySqlContext> options)
        : base(options)
    {
    }

    public MySqlContext()
    {
        
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
}