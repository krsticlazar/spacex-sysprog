namespace spacex_sysprog.Core;

public class LaunchQueryParameters
{
    public bool? Success { get; set; }
    public bool? Upcoming { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? NameContains { get; set; }
    public int Limit { get; set; } = 10; // 1..50
    public string Sort { get; set; } = "desc"; // by date_utc

    public string ToCacheKey()
    {
        return $"s:{Success?.ToString() ?? "-"}|u:{Upcoming?.ToString() ?? "-"}|f:{From:yyyy-MM-dd}|t:{To:yyyy-MM-dd}|n:{NameContains ?? "-"}|l:{Limit}|o:{Sort}";
    }
}