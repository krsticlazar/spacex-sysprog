using System.Text.Json;
using System.Text.Json.Nodes;
using spacex_sysprog.Core;

namespace spacex_sysprog.Infrastructure.Mappers;

public static class LaunchMapper
{
    // Maps SpaceX /launches/query odgovor DTOa sa manjom projekcijom
    public static LaunchQueryResult MapLaunches(string spacexJson)
    {
        var root = JsonNode.Parse(spacexJson)!.AsObject();
        int total = root["total"]?.GetValue<int>() ?? 0;
        var docs = new List<LaunchDto>();
        var arr = root["docs"] as JsonArray ?? new JsonArray();
        foreach (var n in arr)
        {
            if (n is not JsonObject o) continue;
            var dto = new LaunchDto
            {
                Id = o["id"]?.GetValue<string>() ?? string.Empty,
                Name = o["name"]?.GetValue<string>() ?? string.Empty,
                DateUtc = o["date_utc"]?.GetValue<DateTime>() ?? default,
                Success = o["success"]?.GetValue<bool?>(),
                Upcoming = o["upcoming"]?.GetValue<bool>() ?? false,
                RocketId = o["rocket"]?.GetValue<string?>(),
                Details = o["details"]?.GetValue<string?>(),
                PatchSmall = o["links"]?[("patch")]?[("small")]?.GetValue<string?>(),
                Article = o["links"]?[("article")]?.GetValue<string?>(),
                Wikipedia = o["links"]?[("wikipedia")]?.GetValue<string?>(),
                Webcast = o["links"]?[("webcast")]?.GetValue<string?>()
            };
            docs.Add(dto);
        }
        return new LaunchQueryResult { Total = total, Docs = docs };
    }
}