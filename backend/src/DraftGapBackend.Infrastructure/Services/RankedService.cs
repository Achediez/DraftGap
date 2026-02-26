using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Ranked;
using DraftGapBackend.Domain.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

/// <summary>
/// Servicio para obtener estadísticas de ranked del usuario.
/// Responsabilidades:
/// - Proveer stats de Solo/Duo queue (RANKED_SOLO_5x5)
/// - Proveer stats de Flex queue (RANKED_FLEX_SR)
/// - Formatear datos: tier, rank, LP, wins, losses, winrate
/// Datos provenientes de la tabla: player_ranked_stats
/// </summary>
public class RankedService : IRankedService
{
    private readonly IUserRepository _userRepository;
    private readonly IRankedRepository _rankedRepository;
    private readonly ILogger<RankedService> _logger;

    public RankedService(
        IUserRepository userRepository,
        IRankedRepository rankedRepository,
        ILogger<RankedService> logger)
    {
        _userRepository = userRepository;
        _rankedRepository = rankedRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las estadísticas de ranked del usuario para ambas colas.
    /// Busca específicamente:
    /// - RANKED_SOLO_5x5: Cola competitiva individual/duo
    /// - RANKED_FLEX_SR: Cola competitiva flex (grupos de 5)
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Stats de ranked o null si el usuario no tiene PUUID vinculado</returns>
    public async Task<RankedStatsDto?> GetUserRankedStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RiotPuuid))
            return null;

        var rankedStats = await _rankedRepository.GetPlayerRankedStatsAsync(user.RiotPuuid, cancellationToken);
        var soloQueue = rankedStats.FirstOrDefault(r => r.QueueType == "RANKED_SOLO_5x5");
        var flexQueue = rankedStats.FirstOrDefault(r => r.QueueType == "RANKED_FLEX_SR");

        return new RankedStatsDto
        {
            SoloQueue = soloQueue != null ? MapToRankedQueueStatsDto(soloQueue) : null,
            FlexQueue = flexQueue != null ? MapToRankedQueueStatsDto(flexQueue) : null
        };
    }

    private RankedQueueStatsDto MapToRankedQueueStatsDto(Domain.Entities.PlayerRankedStat stat)
    {
        return new RankedQueueStatsDto
        {
            QueueType = stat.QueueType,
            Tier = stat.Tier,
            Rank = stat.Rank,
            LeaguePoints = stat.LeaguePoints ?? 0,
            Wins = stat.Wins,
            Losses = stat.Losses,
            TotalGames = stat.TotalGames,
            Winrate = Math.Round(stat.Winrate, 2),
            UpdatedAt = stat.UpdatedAt
        };
    }
}
