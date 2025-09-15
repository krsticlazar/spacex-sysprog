using System.Net;
using System.Text;
using System.Text.Json;
using System.Diagnostics;  
using spacex_sysprog.Core;
using spacex_sysprog.Core.Interfaces;
using spacex_sysprog.Infrastructure.Logging;
using spacex_sysprog.Infrastructure.Cache;  

namespace spacex_sysprog.Web;

public class WebServer
{
    private readonly HttpListener _listener = new();
    private readonly ILaunchService _service;
    private readonly Logger _logger;
    private readonly CacheManager _cache; // ← NOVI

    public WebServer(string prefix, ILaunchService service, CacheManager cache, Logger logger)
    {
        _service = service;
        _logger = logger;
        _cache = cache; // ← NOVI
        _listener.Prefixes.Add(prefix);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _logger.Info("Server pokrenut. CTRL+C za prekid rada.");
        while (true)
        {
            var ctx = await _listener.GetContextAsync();
            _ = Task.Run(() => HandleAsync(ctx));
        }
    }

    private async Task HandleAsync(HttpListenerContext ctx)
    {
        var sw = Stopwatch.StartNew();  
        var req = ctx.Request;
        var res = ctx.Response;
        string path = req.Url?.AbsolutePath ?? "(null)";
        string query = req.Url?.Query ?? "";
        _logger.Info($"REQ {req.HttpMethod} {path}{query} from {req.RemoteEndPoint}");  

        try
        {
            if (req.Url == null)
            {
                await WriteJson(res, new { error = "Invalid URL" }, HttpStatusCode.BadRequest);
                _logger.Warn($"RES 400 {path}{query} ({sw.ElapsedMilliseconds} ms)");  
                return;
            }

            switch (path.ToLowerInvariant())
            {
                case "/health":
                    await WriteJson(res, new { status = "ok" });
                    _logger.Info($"RES 200 /health ({sw.ElapsedMilliseconds} ms)");  
                    break;

                case "/launches":
                    await HandleLaunchesAsync(req, res);
                    _logger.Info($"RES {res.StatusCode} /launches ({sw.ElapsedMilliseconds} ms)");  
                    break;

                default:
                    await WriteJson(res, new { error = "Not found" }, HttpStatusCode.NotFound);
                    _logger.Warn($"RES 404 {path} ({sw.ElapsedMilliseconds} ms)");  
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex.ToString());
            try
            {
                await WriteJson(ctx.Response, new { error = "Internal server error" }, HttpStatusCode.InternalServerError);
                _logger.Error($"RES 500 {path}{query} ({sw.ElapsedMilliseconds} ms)");  
            }
            catch { }
        }
    }

    private async Task HandleLaunchesAsync(HttpListenerRequest req, HttpListenerResponse res)
    {
        var q = req.QueryString;
        var p = new LaunchQueryParameters();

        if (bool.TryParse(q.Get("success"), out var success)) p.Success = success;
        if (bool.TryParse(q.Get("upcoming"), out var upcoming)) p.Upcoming = upcoming;
        if (DateTime.TryParse(q.Get("from"), out var from)) p.From = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        if (DateTime.TryParse(q.Get("to"), out var to)) p.To = DateTime.SpecifyKind(to, DateTimeKind.Utc);
        var name = q.Get("name");
        if (!string.IsNullOrWhiteSpace(name)) p.NameContains = name.Trim();
        if (int.TryParse(q.Get("limit"), out var limit)) p.Limit = Math.Clamp(limit, 1, 50);
        var sort = q.Get("sort");
        if (!string.IsNullOrWhiteSpace(sort)) p.Sort = sort.Equals("asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";

        string responseKey = "RESP:" + p.ToCacheKey();

        // Ako već imamo gotov JSON, vrati odmah
        if (_cache.TryGet(responseKey, out var cachedJson))
        {
            _logger.Info($"Cache HIT (response) {responseKey}");
            await WriteRawJson(res, cachedJson);
            return;
        }

        var result = await _service.QueryLaunchesAsync(p);
        var finalJson = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

        _cache.Set(responseKey, finalJson);
        await WriteRawJson(res, finalJson);
    }

    // Helper koji upisuje već pripremljen JSON string (bez ponovne serijalizacije)
    private static async Task WriteRawJson(HttpListenerResponse res, string json, HttpStatusCode code = HttpStatusCode.OK)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        res.StatusCode = (int)code;
        res.ContentType = "application/json";
        res.ContentEncoding = Encoding.UTF8;
        res.ContentLength64 = bytes.Length;
        await res.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        res.Close();
    }

    private static async Task WriteJson(HttpListenerResponse res, object obj, HttpStatusCode code = HttpStatusCode.OK)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        await WriteRawJson(res, json, code);
    }
}
