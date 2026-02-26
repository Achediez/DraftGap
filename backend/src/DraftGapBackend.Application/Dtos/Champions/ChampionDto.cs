namespace DraftGapBackend.Application.Dtos.Champions;

/// <summary>
/// Champion static data
/// </summary>
public class ChampionDto
{
    public int ChampionId { get; set; }
    public string ChampionKey { get; set; } = string.Empty;
    public string ChampionName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? ImageUrl { get; set; }
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Champion statistics for a player
/// </summary>
public class ChampionStatsDto
{
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double Winrate { get; set; }
    public double AvgKills { get; set; }
    public double AvgDeaths { get; set; }
    public double AvgAssists { get; set; }
    public double AvgKda { get; set; }
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalAssists { get; set; }
}
