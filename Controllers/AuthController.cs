using Microsoft.AspNetCore.Mvc;
using OnlineShop.Services;

namespace OnlineShop.Controllers;

public class AuthController : Controller
{
    private readonly UserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthController(UserService userService, IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult Index()
    {
        return View("PostLogin");
    }

    public IActionResult Login()
    {
        return View("PostLogin");
    }

    public IActionResult Register()
    {
        return View("PostRegister");
    }

    public async Task<IActionResult> PostLogin(string email, string password)
    {
        await _userService.LoginUser(email, password);
        return Redirect("/");
    }

    public async Task<IActionResult> PostRegister(string email, string password)
    {
        await _userService.RegisterUser(email, password);
        return Redirect("/");
    }
    
    public IActionResult Logout()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("dasToken", "",  new CookieOptions
        {
            Expires = DateTime.UtcNow.AddDays(-1)
        });
        return Redirect("/");
    }
}