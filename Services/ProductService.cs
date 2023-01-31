using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Services;

public class ProductService
{
    private readonly IDbContext _dbContext;

    public ProductService(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _dbContext.Products.ToListAsync();
    }

    public async Task<List<Product>> GetAllProductsIncludingOrderAsync()
    {
        return await _dbContext.Products.Include(x => x.Orders).ToListAsync();
    }

    public async Task CreateProduct(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteProduct(int id)
    {
        var productToRemove = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (productToRemove is null)
            return;
        _dbContext.Products.Remove(productToRemove);
        await _dbContext.SaveChangesAsync();
    }
}