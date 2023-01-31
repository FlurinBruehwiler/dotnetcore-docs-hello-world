using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Exceptions;
using OnlineShop.Models;
using OnlineShop.Services;

namespace OnlineShop.Controllers;

public class HomeController : Controller
{
    private readonly ProductService _productService;
    private readonly OrderService _orderService;
    private readonly UserService _userService;
    private readonly IDbContext _dbContext;

    public HomeController(ProductService productService, OrderService orderService, UserService userService,
        IDbContext dbContext)
    {
        _productService = productService;
        _orderService = orderService;
        _userService = userService;
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userService.GetUser();
        var products = await _productService.GetAllProductsIncludingOrderAsync();
        if (user is not null)
        {
            var order = await _dbContext.Orders.Where(x => x.UserId == user.Id && x.IsCompleted == false)
                .Include(x => x.Products)
                .FirstOrDefaultAsync();
            
            if (order is not null)
                ViewData["totalPrice"] = order.Products.Sum(x => x.Price);
            ViewData["order"] = order;
            
        }
        
        ViewData["products"] = products;
        ViewData["user"] = user;
        ViewData["order"] = null;
        
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> AddToOrder(int id)
    {
        await _orderService.AddProductToOrder(id);
        return Redirect("/");
    }

    public async Task<IActionResult> Checkout()
    {
        var user = await _userService.GetUser();

        if (user is null)
            throw new BadRequestException("User must be logged in to checkout");

        var order = await _dbContext.Orders.Where(x => x.UserId == user.Id && x.IsCompleted == false)
            .Include(x => x.Products).FirstOrDefaultAsync();

        if (order is null || order.Products.Count == 0)
            throw new BadRequestException("There is no order to checkout");

        order.IsCompleted = true;

        await _dbContext.SaveChangesAsync();

        return Redirect($"/Home/CheckoutComplete/{order.Id}");
    }
    
    public async Task<IActionResult> CheckoutComplete(int id)
    {
        var user = await _userService.GetUser();

        if (user is null)
            return NotFound();

        var order = await _dbContext.Orders
            .Where(x => x.Id == id && x.UserId == user.Id)
            .Include(x => x.Products)
            .FirstOrDefaultAsync();

        if (order is null)
            return NotFound();

        order.IsCompleted = true;

        ViewData["products"] = order.Products;
        
        return View("CheckoutComplete");
    }

    public async Task<IActionResult> RemoveFromOrder(int id)
    {
        await _orderService.RemoveProductFromOrder(id);
        return Redirect("/");
    }
}