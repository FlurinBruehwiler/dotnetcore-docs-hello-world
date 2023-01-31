using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using static System.Text.Encoding;

namespace OnlineShop.Services;

public class AuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DatabaseService _databaseService;

    public AuthService(IHttpContextAccessor httpContextAccessor, DatabaseService databaseService)
    {
        _httpContextAccessor = httpContextAccessor;
        _databaseService = databaseService;
    }

    public string CreateToken(string email, bool isAdmin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email)
        };

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var key = new SymmetricSecurityKey(UTF8.GetBytes(_databaseService.GetActiveDb().Secret));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(100),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(UTF8.GetBytes(password));
    }

    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(UTF8.GetBytes(password));
        return computedHash.SequenceEqual(passwordHash);
    }

    public void AppendAccessToken(string email, bool isAdmin)
    {
        var httpResponse = _httpContextAccessor.HttpContext?.Response;

        if (httpResponse is null)
            throw new Exception("HttpResponse was null");

        httpResponse.Cookies.Append("dasToken", CreateToken(email, isAdmin), new CookieOptions
        {
            SameSite = SameSiteMode.Strict,
            Secure = true,
            Domain = null,
            HttpOnly = true,
            IsEssential = true,
            Expires = DateTime.UtcNow.AddDays(100)
        });
        httpResponse.Headers.AccessControlAllowCredentials = "true";
    }
}