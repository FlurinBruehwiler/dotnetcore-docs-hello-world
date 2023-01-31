using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OnlineShop.Configuration;
using OnlineShop.Data;
using OnlineShop.Exceptions;
using OnlineShop.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(options => options.FileProviders.Add(
        new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)));

builder.Services.Configure<DatabaseConfiguration>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<StripePaymentConfiguration>(builder.Configuration.GetSection("StripePayment"));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<IDbContext>(provider =>
{
    var fileStorage = provider.GetRequiredService<DatabaseService>();

    var dbToUse = fileStorage.GetActiveDb();

    return dbToUse.CreateDbConnectionFactory(provider);
});

builder.Services.AddSingleton<DatabaseService>();

builder.Services.AddDbContext<MySqlContext>((provider, options) =>
{
    var dbService = provider.GetRequiredService<IOptions<DatabaseConfiguration>>();
    var db = dbService.Value.Databases.FirstOrDefault(x => x.Name == "MariaDb");
    if (db is null)
        throw new Exception("No mariadb configured");

    options.UseSqlite(db.ConnectionString);
});

builder.Services.AddDbContext<SqliteContext>((provider, options) =>
{
    var dbService = provider.GetRequiredService<IOptions<DatabaseConfiguration>>();
    var db = dbService.Value.Databases.FirstOrDefault(x => x.Name == "Sqlite");
    if (db is null)
        throw new Exception("No sqlite configured");

    options.UseSqlite(db.ConnectionString);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var mariadbContext = scope.ServiceProvider.GetRequiredService<MySqlContext>();
    mariadbContext.Database.Migrate();
    
    var sqliteContext = scope.ServiceProvider.GetRequiredService<SqliteContext>();
    sqliteContext.Database.Migrate();
}

app.UseExceptionHandler(appError =>
{
   appError.Run(async context =>
   {
       var exceptionHandlerPathFeature =
           context.Features.Get<IExceptionHandlerPathFeature>();

       if (exceptionHandlerPathFeature?.Error is not BadRequestException exception)
           return;

       await context.Response.WriteAsJsonAsync(exception.Message);
   }); 
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();