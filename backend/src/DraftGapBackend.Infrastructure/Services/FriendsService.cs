using DraftGapBackend.Application.Dtos.Dashboard;
using DraftGapBackend.Application.Dtos.Friends;
using DraftGapBackend.Application.Dtos.Users;
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
/// Servicio para búsqueda de usuarios y funcionalidad de amigos.
/// Responsabilidades:
/// - Buscar usuarios registrados por Riot ID
/// - Proveer información pública de summoner
/// - Base para futura funcionalidad de sistema de amigos
/// </summary>
public class FriendsService : IFriendsService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IRankedRepository _rankedRepository;
    private readonly ILogger<FriendsService> _logger;

    public FriendsService(
        IUserRepository userRepository,
        IPlayerRepository playerRepository,
        IMatchRepository matchRepository,
        IRankedRepository rankedRepository,
        ILogger<FriendsService> logger)
    {
        _userRepository = userRepository;
        _playerRepository = playerRepository;
        _matchRepository = matchRepository;
        _rankedRepository = rankedRepository;
        _logger = logger;
    }

    /// <summary>
    /// Busca un usuario en la base de datos por su Riot ID.
    /// Útil para:
    /// - Comparar stats con otros jugadores
    /// - Agregar amigos (futura funcionalidad)
    /// - Verificar si un jugador está registrado
    /// </summary>
    /// <param name="riotId">Riot ID en formato GameName#TAG</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del usuario o null si no está registrado</returns>
    public async Task<UserSearchResultDto?> SearchUserByRiotIdAsync(string riotId, CancellationToken cancellationToken = default)
    {
        // Buscar en la tabla users por riot_id
        var user = await _userRepository.GetByRiotIdAsync(riotId);
        if (user == null)
            return null;

        var player = !string.IsNullOrEmpty(user.RiotPuuid)
            ? await _playerRepository.GetByPuuidAsync(user.RiotPuuid, cancellationToken)
            : null;

        return new UserSearchResultDto
        {
            UserId = user.UserId,
            RiotId = user.RiotId ?? string.Empty,
            Region = user.Region ?? "euw1",
            SummonerName = player?.SummonerName,
            ProfileIconId = player?.ProfileIconId,
            SummonerLevel = player?.SummonerLevel,
            IsActive = user.IsActive
        };
    }

    /// <summary>
    /// Obtiene detalles completos de un usuario por su Riot ID (búsqueda case-insensitive).
    /// Agrega datos de:
    /// - User (básico): userId, email, riotId, region, lastSync
    /// - Summoner: puuid, name, level, icon (si tiene PUUID vinculado)
    /// - RankedOverview: stats de Solo/Duo y Flex (si tiene datos)
    /// - RecentMatches: últimas 10 partidas (array vacío si no hay)
    /// - TopChampions: top 5 más jugados, últimas 50 partidas (array vacío si no hay)
    /// </summary>
    /// <param name="riotId">Riot ID en formato GameName#TAG</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Detalles agregados del usuario o null si no existe</returns>
    public async Task<UserDetailsByRiotIdDto?> GetUserDetailsByRiotIdAsync(string riotId, CancellationToken cancellationToken = default)
    {
        // Búsqueda case-insensitive en la base de datos
        var user = await _userRepository.GetByRiotIdAsync(riotId);
        if (user == null)
        {
            _logger.LogWarning("User not found with Riot ID: {RiotId}", riotId);
            return null;
        }

        var result = new UserDetailsByRiotIdDto
        {
            UserId = user.UserId,
            Email = user.Email,
            RiotId = user.RiotId ?? string.Empty,
            Region = user.Region,
            LastSync = user.LastSync
        };

        // Si no tiene PUUID vinculado, solo devolver datos básicos
        if (string.IsNullOrEmpty(user.RiotPuuid))
        {
            _logger.LogInformation("User {RiotId} has no linked Riot account (no PUUID)", riotId);
            return result;
        }

        var puuid = user.RiotPuuid;

        // ===== SUMMONER INFO =====
        var player = await _playerRepository.GetByPuuidAsync(puuid, cancellationToken);
        if (player != null)
        {
            result.Summoner = new UserSummonerInfoDto
            {
                Puuid = player.Puuid,
                SummonerName = player.SummonerName ?? string.Empty,
                ProfileIconId = player.ProfileIconId ?? 0,
                SummonerLevel = player.SummonerLevel ?? 0
            };
        }

        // ===== RANKED OVERVIEW =====
        var rankedStats = await _rankedRepository.GetPlayerRankedStatsAsync(puuid, cancellationToken);
        if (rankedStats.Any())
        {
            var soloQueue = rankedStats.FirstOrDefault(r => r.QueueType == "RANKED_SOLO_5x5");
            var flexQueue = rankedStats.FirstOrDefault(r => r.QueueType == "RANKED_FLEX_SR");

            result.RankedOverview = new RankedOverviewDto
            {
                SoloQueue = soloQueue != null ? MapToRankedQueueDto(soloQueue) : null,
                FlexQueue = flexQueue != null ? MapToRankedQueueDto(flexQueue) : null
            };
        }

        // ===== RECENT MATCHES (últimas 10) =====
        var recentParticipants = await _matchRepository.GetUserMatchParticipantsAsync(puuid, 0, 10, cancellationToken);
        result.RecentMatches = recentParticipants.Select(p => new RecentMatchDto
        {
            MatchId = p.MatchId,
            GameCreation = p.Match?.GameCreation ?? 0,
            GameDuration = p.Match?.GameDuration ?? 0,
            ChampionName = p.ChampionName ?? string.Empty,
            Win = p.Win,
            Kills = p.Kills,
            Deaths = p.Deaths,
            Assists = p.Assists,
            Kda = p.Deaths > 0 ? Math.Round((p.Kills + p.Assists) / (double)p.Deaths, 2) : p.Kills + p.Assists,
            TeamPosition = p.TeamPosition ?? string.Empty
        }).ToList();

        // ===== TOP CHAMPIONS (top 5, basado en últimas 50 partidas) =====
        var championParticipants = await _matchRepository.GetUserMatchParticipantsAsync(puuid, 0, 50, cancellationToken);
        var championGroups = championParticipants
            .GroupBy(p => p.ChampionName)
            .Select(g => new
            {
                ChampionName = g.Key ?? "Unknown",
                Games = g.Count(),
                Wins = g.Count(p => p.Win),
                Kills = g.Sum(p => p.Kills),
                Deaths = g.Sum(p => p.Deaths),
                Assists = g.Sum(p => p.Assists)
            })
            .OrderByDescending(c => c.Games)
            .Take(5)
            .ToList();

        result.TopChampions = championGroups.Select(c => new TopChampionDto
        {
            ChampionId = 0, // No tenemos championId aquí, se agregará en futuras mejoras
            ChampionName = c.ChampionName,
            GamesPlayed = c.Games,
            Wins = c.Wins,
            Winrate = c.Games > 0 ? Math.Round((c.Wins / (double)c.Games) * 100, 1) : 0,
            AvgKda = c.Deaths > 0 ? Math.Round((c.Kills + c.Assists) / (double)c.Deaths, 2) : c.Kills + c.Assists
        }).ToList();

        _logger.LogInformation("Retrieved details for user {RiotId}: {MatchCount} matches, {ChampionCount} champions",
            riotId, result.RecentMatches.Count, result.TopChampions.Count);

        return result;
    }

    /// <summary>
    /// Mapea PlayerRankedStat a RankedQueueDto
    /// </summary>
    private RankedQueueDto MapToRankedQueueDto(Domain.Entities.PlayerRankedStat stat)
    {
        var totalGames = stat.Wins + stat.Losses;
        return new RankedQueueDto
        {
            QueueType = stat.QueueType ?? string.Empty,
            Tier = stat.Tier,
            Rank = stat.Rank,
            LeaguePoints = stat.LeaguePoints ?? 0,
            Wins = stat.Wins,
            Losses = stat.Losses,
            TotalGames = totalGames,
            Winrate = totalGames > 0 ? Math.Round((stat.Wins / (double)totalGames) * 100, 1) : 0
        };
    }
}
