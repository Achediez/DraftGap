using System;

namespace DraftGapBackend.Application.Dtos.Ranked;

/// <summary>
/// Ranked statistics for all queues
/// </summary>
public class RankedStatsDto
{
    public RankedQueueStatsDto? SoloQueue { get; set; }
    public RankedQueueStatsDto? FlexQueue { get; set; }
}

/// <summary>
/// Ranked statistics for a specific queue
/// </summary>
public class RankedQueueStatsDto
{
    public string QueueType { get; set; } = string.Empty;
    public string? Tier { get; set; }
    public string? Rank { get; set; }
    public int LeaguePoints { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int TotalGames { get; set; }
    public double Winrate { get; set; }
    public DateTime UpdatedAt { get; set; }
}
