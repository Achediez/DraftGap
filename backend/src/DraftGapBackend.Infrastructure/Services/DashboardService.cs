using DraftGapBackend.Application.Dashboard;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

/// <summary>
/// Servicio para generar el resumen del dashboard del usuario.
/// Responsabilidades:
/// - Agregar datos de ranked (Solo/Duo y Flex)
/// - Obtener últimas partidas jugadas
/// - Calcular estadísticas de rendimiento (KDA, winrate)
/// - Identificar top champions más jugados
/// Combina datos de múltiples tablas: users, player_ranked_stats, match_participants, matches
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IRankedRepository _rankedRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IUserRepository userRepository,
        IRankedRepository rankedRepository,
        IMatchRepository matchRepository,
        ILogger<DashboardService> logger)
    {
        _userRepository = userRepository;
        _rankedRepository = rankedRepository;
        _matchRepository = matchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el resumen completo del dashboard para un usuario.
    /// Proceso:
    /// 1. Valida que el usuario tenga PUUID vinculado
    /// 2. Carga ranked stats de Solo/Duo y Flex
    /// 3. Obtiene últimas 10 partidas
    /// 4. Calcula estadísticas de rendimiento (basado en últimas 20 partidas)
    /// 5. Identifica top 5 champions más jugados (basado en últimas 50 partidas)
    /// </summary>
    /// <param name="userId">ID del usuario autenticado</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resumen completo del dashboard</returns>
    /// <exception cref="InvalidOperationException">Si el usuario no existe o no tiene cuenta de Riot vinculada</exception>
    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RiotPuuid))
            throw new InvalidOperationException("User not found or has no linked Riot account");

        var puuid = user.RiotPuuid;

        // ===== SECCIÓN 1: RANKED STATS =====
        // Obtiene stats de Solo/Duo (RANKED_SOLO_5x5) y Flex (RANKED_FLEX_SR)
        var rankedStats = await _rankedRepository.GetPlayerRankedStatsAsync(puuid, cancellationToken);
        var soloQueue = rankedStats.FirstOrDefault(r => r.QueueType == "RANKED_SOLO_5x5");
        var flexQueue = rankedStats.FirstOrDefault(r => r.QueueType == "RANKED_FLEX_SR");

        var rankedOverview = new RankedOverviewDto
        {
            SoloQueue = soloQueue != null ? MapToRankedQueueDto(soloQueue) : null,
            FlexQueue = flexQueue != null ? MapToRankedQueueDto(flexQueue) : null
        };

        // ===== SECCIÓN 2: RECENT MATCHES =====
        // Obtiene las últimas 10 partidas ordenadas por fecha descendente
        // Incluye información básica: champion, resultado, KDA, posición
        var recentParticipants = await _matchRepository.GetUserMatchParticipantsAsync(puuid, 0, 10, cancellationToken);
        var recentMatches = recentParticipants.Select(p => new RecentMatchDto
        {
            MatchId = p.MatchId,
            GameCreation = p.Match?.GameCreation ?? 0,
            GameDuration = p.Match?.GameDuration ?? 0,
            ChampionName = p.ChampionName,
            Win = p.Win,
            Kills = p.Kills,
            Deaths = p.Deaths,
            Assists = p.Assists,
            Kda = p.Deaths > 0 ? Math.Round((double)(p.Kills + p.Assists) / p.Deaths, 2) : p.Kills + p.Assists,
            TeamPosition = p.TeamPosition
        }).ToList();

        // ===== SECCIÓN 3: PERFORMANCE STATS =====
        // Calcula estadísticas generales basadas en las últimas 20 partidas
        // Incluye: total de partidas, wins, losses, winrate, promedios de K/D/A y KDA
        var allParticipants = await _matchRepository.GetUserMatchParticipantsAsync(puuid, 0, 20, cancellationToken);
        var performanceStats = CalculatePerformanceStats(allParticipants);

        // ===== SECCIÓN 4: TOP CHAMPIONS =====
        // Identifica los 5 campeones más jugados basado en las últimas 50 partidas
        // Agrupa por champion y calcula: games, wins, winrate, KDA promedio
        var championParticipants = await _matchRepository.GetUserMatchParticipantsAsync(puuid, 0, 50, cancellationToken);
        var topChampions = championParticipants
            .GroupBy(p => new { p.ChampionId, p.ChampionName })
            .Select(g => new TopChampionDto
            {
                ChampionId = g.Key.ChampionId,
                ChampionName = g.Key.ChampionName,
                GamesPlayed = g.Count(),
                Wins = g.Count(p => p.Win),
                Winrate = g.Count() > 0 ? Math.Round((double)g.Count(p => p.Win) / g.Count() * 100, 2) : 0,
                AvgKda = g.Average(p => p.Deaths > 0 ? (double)(p.Kills + p.Assists) / p.Deaths : p.Kills + p.Assists)
            })
            .OrderByDescending(c => c.GamesPlayed)
            .Take(5)
            .ToList();

        return new DashboardSummaryDto
        {
            RankedOverview = rankedOverview,
            RecentMatches = recentMatches,
            PerformanceStats = performanceStats,
            TopChampions = topChampions
        };
    }

    /// <summary>
    /// Mapea una entidad PlayerRankedStat a DTO de respuesta.
    /// Calcula winrate y formatea datos para el cliente.
    /// </summary>
    private RankedQueueDto MapToRankedQueueDto(Domain.Entities.PlayerRankedStat stat)
    {
        return new RankedQueueDto
        {
            QueueType = stat.QueueType,
            Tier = stat.Tier,
            Rank = stat.Rank,
            LeaguePoints = stat.LeaguePoints ?? 0,
            Wins = stat.Wins,
            Losses = stat.Losses,
            TotalGames = stat.TotalGames,
            Winrate = Math.Round(stat.Winrate, 2)
        };
    }

    /// <summary>
    /// Calcula estadísticas de rendimiento agregadas.
    /// Fórmula KDA: (Kills + Assists) / Deaths (si Deaths > 0, sino Kills + Assists)
    /// </summary>
    /// <param name="participants">Lista de participaciones del usuario</param>
    /// <returns>Estadísticas agregadas o null si no hay partidas</returns>
    private PerformanceStatsDto? CalculatePerformanceStats(System.Collections.Generic.List<Domain.Entities.MatchParticipant> participants)
    {
        if (!participants.Any())
            return null;

        var totalMatches = participants.Count;
        var wins = participants.Count(p => p.Win);

        return new PerformanceStatsDto
        {
            TotalMatches = totalMatches,
            Wins = wins,
            Losses = totalMatches - wins,
            Winrate = Math.Round((double)wins / totalMatches * 100, 2),
            AvgKills = Math.Round(participants.Average(p => p.Kills), 2),
            AvgDeaths = Math.Round(participants.Average(p => p.Deaths), 2),
            AvgAssists = Math.Round(participants.Average(p => p.Assists), 2),
            AvgKda = Math.Round(participants.Average(p => p.Deaths > 0 ? (double)(p.Kills + p.Assists) / p.Deaths : p.Kills + p.Assists), 2)
        };
    }
}
