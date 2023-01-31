using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineShop.Configuration;
using OnlineShop.Data;
using OnlineShop.Exceptions;
using OnlineShop.Models;
using OnlineShop.Services;

namespace OnlineShop.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ProductService _productService;
    private readonly IOptions<DatabaseConfiguration> _dbOptions;
    private readonly DatabaseService _databaseService;
    private readonly IDbContext _dbContext;
    private readonly UserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminController(ProductService productService, IOptions<DatabaseConfiguration> dbOptions,
        DatabaseService databaseService, IDbContext dbContext, UserService userService, IHttpContextAccessor httpContextAccessor)
    {
        _productService = productService;
        _dbOptions = dbOptions;
        _databaseService = databaseService;
        _dbContext = dbContext;
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["products"] = await _productService.GetAllProductsAsync();
        var activeDb = _databaseService.FileStorage.ActiveDb;
        ViewData["dbs"] = _dbOptions.Value.Databases.Select(x =>
        {
            var str = x.Name;

            if (x.Name == activeDb)
                str += " (active)";

            return str;
        }).ToList();
        ViewData["users"] = await _dbContext.Users.Include(x => x.Orders).ToListAsync();
        return View("Index");
    }

    public async Task<IActionResult> CreateProduct(string name, string? description, string? images, int amount, int price)
    {
        if (string.IsNullOrEmpty(name))
            throw new BadRequestException("Product must have a name");
        
        await _productService.CreateProduct(new Product
        {
            Name = name,
            Description = description ?? string.Empty,
            Amount = amount,
            Images = images ?? string.Empty,
            Price = price
        });

        return Redirect("/Admin");
    }

    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProduct(id);
        return Redirect("/Admin");
    }

    public async Task<IActionResult> ChangeDb(string name)
    {
        _databaseService.FileStorage.ActiveDb = name.Split(' ').First();
        await _databaseService.SaveChangesAsync();
        return Redirect("/");
    }

    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _userService.GetUserById(id);

        if (user is null)
            throw new BadRequestException("User does not exist");

  

        _dbContext.Users.Remove(user);

        await _dbContext.SaveChangesAsync();
        
        if (user.IsAdmin)
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Append("dasToken", "",  new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(-1)
            });
            return Redirect("/");
        }

        return Redirect("/Admin");
    }
}