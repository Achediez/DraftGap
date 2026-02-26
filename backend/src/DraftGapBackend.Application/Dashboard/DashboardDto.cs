using System;
using System.Collections.Generic;

namespace DraftGapBackend.Application.Dashboard;

/// <summary>
/// DTO del resumen completo del dashboard.
/// Agrega datos de 4 fuentes:
/// - rankedOverview: Stats de Solo/Duo y Flex
/// - recentMatches: Últimas 10 partidas
/// - performanceStats: Promedios K/D/A (últimas 20 partidas)
/// - topChampions: Top 5 campeones más jugados (últimas 50 partidas)
/// Usado en: GET /api/dashboard/summary
/// Este es el endpoint más usado, considerar implementar caching.
/// </summary>
public class DashboardSummaryDto
{
    public RankedOverviewDto? RankedOverview { get; set; }
    public List<RecentMatchDto> RecentMatches { get; set; } = [];
    public PerformanceStatsDto? PerformanceStats { get; set; }
    public List<TopChampionDto> TopChampions { get; set; } = [];
}

/// <summary>
/// Resumen de ranked para dashboard.
/// Incluye stats separadas de Solo/Duo (RANKED_SOLO_5x5) y Flex (RANKED_FLEX_SR).
/// Ambas pueden ser null si el usuario no ha jugado ranked.
/// </summary>
public class RankedOverviewDto
{
    public RankedQueueDto? SoloQueue { get; set; }
    public RankedQueueDto? FlexQueue { get; set; }
}

/// <summary>
/// Estadísticas de una cola de ranked específica.
/// Incluye: tier, rank, LP, wins, losses, winrate.
/// Ejemplo: GOLD II, 67 LP, 15W - 10L (60% winrate)
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
/// DTO simplificado para partida reciente en dashboard.
/// Solo incluye datos esenciales para renderizar la lista.
/// Para detalles completos, navegar a /api/matches/{matchId}
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
/// Estadísticas agregadas de rendimiento del jugador.
/// Calculadas sobre las últimas 20 partidas.
/// - totalMatches, wins, losses: Contadores
/// - winrate: Porcentaje de victorias
/// - avgKills/Deaths/Assists: Promedios por partida
/// - avgKDA: Fórmula (Kills + Assists) / Deaths
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
/// DTO de campeón más jugado para dashboard.
/// Top 5 campeones basado en últimas 50 partidas.
/// Incluye: games, wins, losses, winrate, avgKDA
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
