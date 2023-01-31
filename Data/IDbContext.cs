using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OnlineShop.Models;

namespace OnlineShop.Data;

public interface IDbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DatabaseFacade Database { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}