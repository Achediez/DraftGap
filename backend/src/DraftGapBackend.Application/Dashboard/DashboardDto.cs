using System;
using System.Collections.Generic;

namespace DraftGapBackend.Application.Dashboard;

/// <summary>
/// Dashboard summary with ranked stats and recent performance
/// </summary>
public class DashboardSummaryDto
{
    public RankedOverviewDto? RankedOverview { get; set; }
    public List<RecentMatchDto> RecentMatches { get; set; } = [];
    public PerformanceStatsDto? PerformanceStats { get; set; }
    public List<TopChampionDto> TopChampions { get; set; } = [];
}

/// <summary>
/// Ranked overview with Solo/Duo and Flex queue stats
/// </summary>
public class RankedOverviewDto
{
    public RankedQueueDto? SoloQueue { get; set; }
    public RankedQueueDto? FlexQueue { get; set; }
}

/// <summary>
/// Ranked queue information
/// </summary>
public class RankedQueueDto
{
    public string QueueType { get; set; } = string.Empty;
    public string? Tier { get; set; }
    public string? Rank { get; set; }
    public int LeaguePoints { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int TotalGames { get; set; }
    public double Winrate { get; set; }
}

/// <summary>
/// Recent match summary
/// </summary>
public class RecentMatchDto
{
    public string MatchId { get; set; } = string.Empty;
    public long GameCreation { get; set; }
    public int GameDuration { get; set; }
    public string ChampionName { get; set; } = string.Empty;
    public bool Win { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public double Kda { get; set; }
    public string TeamPosition { get; set; } = string.Empty;
}

/// <summary>
/// Overall performance statistics
/// </summary>
public class PerformanceStatsDto
{
    public int TotalMatches { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double Winrate { get; set; }
    public double AvgKills { get; set; }
    public double AvgDeaths { get; set; }
    public double AvgAssists { get; set; }
    public double AvgKda { get; set; }
}

/// <summary>
/// Top played champion statistics
/// </summary>
public class TopChampionDto
{
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = string.Empty;
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public double Winrate { get; set; }
    public double AvgKda { get; set; }
}
