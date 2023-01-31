
using OnlineShop.Data;

namespace OnlineShop.Configuration;

public class DatabaseConfiguration
{
    public required List<DbConfig> Databases { get; set; }
    public required string FileStorageLocation { get; set; }
}

public class DbConfig
{
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }

    public required string Secret { get; set; }

    public IDbContext CreateDbConnectionFactory(IServiceProvider serviceProvider)
    {
        return Name switch
        {
            "MariaDb" => serviceProvider.GetRequiredService<MySqlContext>(),
            "Sqlite" => serviceProvider.GetRequiredService<SqliteContext>(),
            _ => throw new Exception($"There is no db with the name {Name} configured")
        };
    }
}