using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using static System.Text.Encoding;


namespace OnlineShop.Services;

public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly DatabaseService _databaseService;

    public ConfigureJwtBearerOptions(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }
    
    public void Configure(string? name, JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKeyResolver = (_, _, _, _) =>
            {
                return new[]
                {
                    new SymmetricSecurityKey(UTF8.GetBytes(_databaseService.GetActiveDb().Secret!))
                };
            }
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["dasToken"];
                return Task.CompletedTask;
            }
        };
    }
    
    public void Configure(JwtBearerOptions options)
    {
        Configure(string.Empty, options);
    }
}