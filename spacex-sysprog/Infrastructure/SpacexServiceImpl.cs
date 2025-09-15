using System.Net.Http;
using System.Text;
using System.Text.Json;
using spacex_sysprog.Core;
using spacex_sysprog.Core.Interfaces;
using spacex_sysprog.Infrastructure.Cache;
using spacex_sysprog.Infrastructure.Logging;

namespace spacex_sysprog.Infrastructure;

public class SpacexServiceImpl : ILaunchService
{
    private readonly string _baseUrl;
    private readonly CacheManager _cache;
    private readonly Logger _logger;
    private readonly HttpClient _http = new();

    public SpacexServiceImpl(string baseUrl, CacheManager cache, Logger logger)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _cache = cache;
        _logger = logger;
    }

    public async Task<LaunchQueryResult> QueryLaunchesAsync(LaunchQueryParameters p, CancellationToken ct = default)
    {
        var key = p.ToCacheKey();
        if (_cache.TryGet(key, out var cached))
        {
            return Mappers.LaunchMapper.MapLaunches(cached);
        }

        var query = new Dictionary<string, object?>();

        if (p.Success.HasValue) query["success"] = p.Success.Value;
        if (p.Upcoming.HasValue) query["upcoming"] = p.Upcoming.Value;

        var date = new Dictionary<string, string>();
        if (p.From.HasValue) date["$gte"] = p.From.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        if (p.To.HasValue)   date["$lte"] = p.To.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        if (date.Count > 0) query["date_utc"] = date;

        if (!string.IsNullOrWhiteSpace(p.NameContains))
        {
            // case-insensitive
            query["name"] = new Dictionary<string, string> {
                {"$regex", p.NameContains},
                {"$options", "i"}
            };
        }

        var options = new Dictionary<string, object?>
        {
            { "limit", Math.Clamp(p.Limit, 1, 50) },
            { "sort", new Dictionary<string, int> { { "date_utc", p.Sort.Equals("asc", StringComparison.OrdinalIgnoreCase) ? 1 : -1 } } },
            // Mala projekcija za smanjenje optećenja
            { "select", new Dictionary<string, int> {
                { "name", 1 }, { "date_utc", 1 }, { "success", 1 }, { "upcoming", 1 },
                { "rocket", 1 }, { "details", 1 },
                { "links.patch.small", 1 }, { "links.article", 1 }, { "links.wikipedia", 1 }, { "links.webcast", 1 }
            }}
        };

        var body = new { query, options };
        var json = JsonSerializer.Serialize(body);
        var url = $"{_baseUrl}/launches/query";

        _logger.Info($"POST {url} body={json}");
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await _http.PostAsync(url, content, ct);
        var respText = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.Error($"SpaceX API error {(int)resp.StatusCode}: {respText}");
            throw new Exception($"SpaceX API error {(int)resp.StatusCode}");
        }

        _cache.Set(key, respText);
        return Mappers.LaunchMapper.MapLaunches(respText);
    }
}