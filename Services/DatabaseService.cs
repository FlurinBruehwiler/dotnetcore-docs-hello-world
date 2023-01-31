using System.Text.Json;
using Microsoft.Extensions.Options;
using OnlineShop.Configuration;

namespace OnlineShop.Services;

public class DatabaseService
{
    private readonly IOptions<DatabaseConfiguration> _databaseConfigOptions;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public DatabaseService(IOptions<DatabaseConfiguration> databaseConfigOptions)
    {
        _databaseConfigOptions = databaseConfigOptions;
        var text = File.ReadAllText(_databaseConfigOptions.Value.FileStorageLocation);
        FileStorage = JsonSerializer.Deserialize<FileStorage>(text) ?? throw new Exception();
    }

    public async Task SaveChangesAsync()
    {
        var text = JsonSerializer.Serialize(FileStorage, _jsonSerializerOptions);
        await File.WriteAllTextAsync(_databaseConfigOptions.Value.FileStorageLocation, text);
    }

    public DbConfig GetActiveDb()
    {
        return _databaseConfigOptions.Value.Databases.FirstOrDefault(x => x.Name == FileStorage.ActiveDb)
               ?? throw new Exception($"No db with the name {FileStorage.ActiveDb} configured");
    }

    public FileStorage FileStorage { get; private set; }
}

public class FileStorage
{
    public required string ActiveDb { get; set; }
}