using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;

namespace OnlineShop.Data;

public class SqliteContext : DbContext, IDbContext
{
    public SqliteContext(DbContextOptions<SqliteContext> options)
        : base(options)
    {
    }

    public SqliteContext()
    {
        
    }
    
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
}