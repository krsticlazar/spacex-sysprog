namespace spacex_sysprog.Configuration;

public class AppSettings
{
    public ServerSettings Server { get; set; } = new();
    public SpacexSettings Spacex { get; set; } = new();
    public CacheSettings Cache { get; set; } = new();
}

public class ServerSettings
{
    public string Prefix { get; set; } = "http://localhost:5055/";
}

public class SpacexSettings
{
    public string BaseUrl { get; set; } = "https://api.spacexdata.com/v5";
}

public class CacheSettings
{
    public int TtlSeconds { get; set; } = 30;
}