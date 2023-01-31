using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Exceptions;
using OnlineShop.Models;

namespace OnlineShop.Services;

public class OrderService
{
    private readonly IDbContext _dbContext;
    private readonly UserService _userService;

    public OrderService(IDbContext dbContext, UserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task AddProductToOrder(int productId)
    {
        var user = await _userService.GetUser();

        if (user is null)
            throw new BadRequestException("User must be logged in to order a product");

        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);

        if (product is null)
            throw new BadRequestException("Product does not exist");
        
        var existingOrder = await _dbContext.Orders
            .Where(x => x.User.Id == user.Id && x.IsCompleted == false)
            .Include(x => x.Products)
            .FirstOrDefaultAsync();

        if (existingOrder is null)
        {
            existingOrder = new Order
            {
                User = user,
                Products = new List<Product>(),
                Date = DateTime.Now
            };
            _dbContext.Orders.Add(existingOrder);
        }
        else if(existingOrder.Products.FirstOrDefault(x => x.Id == productId) is not null)
        {
            throw new BadRequestException("User already ordered product");
        }
        
        existingOrder.Products.Add(product);

        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveProductFromOrder(int productId)
    {
        var user = await _userService.GetUser();

        if (user is null)
            throw new BadRequestException("User must be logged in to remove a product from an order");

        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);

        if (product is null)
            throw new BadRequestException("Product does not exist");
        
        var order = await _dbContext.Orders
            .Where(x => x.User.Id == user.Id && x.IsCompleted == false)
            .Include(x => x.Products)
            .FirstOrDefaultAsync();

        if (order is null)
            throw new BadRequestException("Cant find open order");
        
        order.Products.Remove(product);

        if (order.Products.Count == 0)
            _dbContext.Orders.Remove(order);

        await _dbContext.SaveChangesAsync();
    }
}