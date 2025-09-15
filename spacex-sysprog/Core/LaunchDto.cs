namespace spacex_sysprog.Core;

public class LaunchDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime DateUtc { get; set; }
    public bool? Success { get; set; }
    public bool Upcoming { get; set; }
    public string? RocketId { get; set; }
    public string? Details { get; set; }
    public string? PatchSmall { get; set; }
    public string? Article { get; set; }
    public string? Wikipedia { get; set; }
    public string? Webcast { get; set; }
}

public class LaunchQueryResult
{
    public int Total { get; set; }
    public List<LaunchDto> Docs { get; set; } = new();
}