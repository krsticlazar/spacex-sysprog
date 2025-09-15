using System.Collections.Concurrent;
using spacex_sysprog.Infrastructure.Logging;

namespace spacex_sysprog.Infrastructure.Cache;

public class CacheManager
{
    private readonly int _ttlSeconds;
    private readonly Logger _logger;
    private readonly ConcurrentDictionary<string, (DateTime storedAt, string payload)> _store = new(); // ← CHANGED

    public CacheManager(int ttlSeconds, Logger logger)
    {
        _ttlSeconds = ttlSeconds;
        _logger = logger;
    }

    public bool TryGet(string key, out string payload)
    {
        payload = string.Empty;
        if (_store.TryGetValue(key, out var entry))
        {
            if ((DateTime.UtcNow - entry.storedAt).TotalSeconds <= _ttlSeconds)
            {
                payload = entry.payload;
                _logger.Info($"Keš HIT: {key}");
                return true;
            }
            _logger.Info($"Keš ISTEKAO: {key}");
            _store.TryRemove(key, out _);
        }
        _logger.Info($"Keš PROMAŠEN: {key}");
        return false;
    }

    public void Set(string key, string payload)
    {
        _store[key] = (DateTime.UtcNow, payload);
    }
}
