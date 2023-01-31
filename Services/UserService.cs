using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Exceptions;
using OnlineShop.Models;

namespace OnlineShop.Services;

public class UserService
{
    private readonly AuthService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDbContext _dbContext;

    public UserService(AuthService authService, IHttpContextAccessor httpContextAccessor,
        IDbContext dbContext)
    {
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public async Task RegisterUser(string email, string password)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (existingUser is not null)
            throw new BadRequestException("Email already taken");

        var firstUser = await _dbContext.Users.FirstOrDefaultAsync();

        var isAdmin = firstUser is null;

        _authService.CreatePasswordHash(password, out var passwordHash, out var passwordSalt);

        _dbContext.Users.Add(new User
        {
            Email = email,
            IsAdmin = isAdmin,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Orders = new List<Order>()
        });

        await _dbContext.SaveChangesAsync();
        
        _authService.AppendAccessToken(email, isAdmin);
    }

    public async Task LoginUser(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);

        if (!_authService.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            throw new BadRequestException("Username and password do not match");

        _authService.AppendAccessToken(user.Email, user.IsAdmin);
    }

    private async Task<User> GetUserByEmailAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user is null)
            throw new BadRequestException("Usere not found");
        return user;
    }

    public string? GetEmail()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
    }

    public async Task<User?> GetUser()
    {
        var email = GetEmail();
        if (email is null)
            return null;
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetUserById(int id)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
    }
}