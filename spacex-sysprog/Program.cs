using System.Text.Json;
using spacex_sysprog.Configuration;
using spacex_sysprog.Infrastructure;
using spacex_sysprog.Web;

namespace spacex_sysprog;

public class Program
{
    public static async Task Main(string[] args)
    {
        var settings = await LoadSettingsAsync();
        var logger = new Infrastructure.Logging.Logger();
        var cache = new Infrastructure.Cache.CacheManager(settings.Cache.TtlSeconds, logger);
        var spacex = new SpacexServiceImpl(settings.Spacex.BaseUrl, cache, logger);
        var server = new WebServer(settings.Server.Prefix, spacex, cache, logger);

        logger.Info($"Pokrećemo server na: {settings.Server.Prefix}");
        await server.StartAsync();
    }

    private static async Task<AppSettings> LoadSettingsAsync()
    {
        var json = await File.ReadAllTextAsync("AppSettings.json");
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var settings = JsonSerializer.Deserialize<AppSettings>(json, opts) ?? throw new Exception("AppSettings.json invalid");
        return settings;
    }
}
