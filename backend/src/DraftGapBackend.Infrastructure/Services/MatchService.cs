using DraftGapBackend.Application.Dtos.Common;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Dtos.Matches;
using DraftGapBackend.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de partidas y su historial.
/// Responsabilidades:
/// - Obtener historial de partidas del usuario con paginación
/// - Aplicar filtros: champion, posición, resultado, fecha, queue
/// - Obtener detalles completos de una partida específica
/// - Calcular KDA y formatear datos para el cliente
/// </summary>
public class MatchService : IMatchService
{
    private readonly IUserRepository _userRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<MatchService> _logger;

    public MatchService(
        IUserRepository userRepository,
        IMatchRepository matchRepository,
        ILogger<MatchService> logger)
    {
        _userRepository = userRepository;
        _matchRepository = matchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene historial de partidas paginado con filtros opcionales.
    /// Implementa paginación eficiente con Skip/Take y filtrado en memoria.
    /// </summary>
    /// <param name="userId">ID del usuario autenticado</param>
    /// <param name="pagination">Parámetros de paginación (page, pageSize)</param>
    /// <param name="filter">Filtros opcionales (champion, position, win, queue)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado con metadata (totalCount, totalPages, hasNext, etc.)</returns>
    public async Task<PaginatedResult<MatchListItemDto>> GetUserMatchesAsync(
        Guid userId,
        PaginationRequest pagination,
        MatchFilterRequest? filter = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RiotPuuid))
            throw new InvalidOperationException("User not found or has no linked Riot account");

        var puuid = user.RiotPuuid;

        // Calcular offset para paginación (page 1 = skip 0, page 2 = skip pageSize, etc.)
        var skip = (pagination.Page - 1) * pagination.PageSize;

        // Obtener count total para metadata de paginación
        var totalCount = await _matchRepository.GetUserMatchCountAsync(puuid, cancellationToken);

        // Obtener participaciones paginadas del usuario
        var participants = await _matchRepository.GetUserMatchParticipantsAsync(puuid, skip, pagination.PageSize, cancellationToken);

        // ===== APLICAR FILTROS =====
        // Los filtros se aplican en memoria después de la paginación
        // Para mejor rendimiento, considerar mover filtros a query SQL en el futuro
        var filteredList = participants.ToList();
        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.ChampionName))
                filteredList = filteredList.Where(p => p.ChampionName.Contains(filter.ChampionName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(filter.TeamPosition))
                filteredList = filteredList.Where(p => p.TeamPosition.Equals(filter.TeamPosition, StringComparison.OrdinalIgnoreCase)).ToList();

            if (filter.Win.HasValue)
                filteredList = filteredList.Where(p => p.Win == filter.Win.Value).ToList();

            if (filter.QueueId > 0 && filter.QueueId != 0)
                filteredList = filteredList.Where(p => p.Match != null && p.Match.QueueId == filter.QueueId).ToList();
        }

        var items = filteredList.Select(p => new MatchListItemDto
        {
            MatchId = p.MatchId,
            GameCreation = p.Match != null ? p.Match.GameCreation : 0,
            GameDuration = p.Match != null ? p.Match.GameDuration : 0,
            ChampionName = p.ChampionName,
            Win = p.Win,
            Kills = p.Kills,
            Deaths = p.Deaths,
            Assists = p.Assists,
            Kda = p.Deaths > 0 ? Math.Round((double)(p.Kills + p.Assists) / p.Deaths, 2) : p.Kills + p.Assists,
            TeamPosition = p.TeamPosition,
            QueueId = p.Match != null ? p.Match.QueueId : 0
        }).ToList();

        return new PaginatedResult<MatchListItemDto>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Obtiene detalles completos de una partida específica.
    /// Incluye todos los participantes agrupados por equipo con stats detalladas:
    /// - Builds (items, runes, summoner spells)
    /// - Estadísticas de combate (damage, vision, gold, CS)
    /// - Resultado individual
    /// </summary>
    /// <param name="matchId">ID de la partida (ej: EUW1_123456)</param>
    /// <param name="userId">ID del usuario solicitante (para validación)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Detalles completos de la partida o null si no existe</returns>
    public async Task<MatchDetailDto?> GetMatchDetailAsync(string matchId, Guid userId, CancellationToken cancellationToken = default)
    {
        var match = await _matchRepository.GetByIdAsync(matchId, cancellationToken);
        if (match == null)
            return null;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        var teams = match.Participants
            .GroupBy(p => p.TeamId)
            .Select(g => new TeamDto
            {
                TeamId = g.Key,
                Win = g.First().Win,
                Participants = g.Select(p => new ParticipantDto
                {
                    Puuid = p.Puuid,
                    RiotIdGameName = p.RiotIdGameName,
                    ChampionId = p.ChampionId,
                    ChampionName = p.ChampionName,
                    ChampLevel = p.ChampLevel,
                    TeamPosition = p.TeamPosition,
                    Win = p.Win,
                    Kills = p.Kills,
                    Deaths = p.Deaths,
                    Assists = p.Assists,
                    Kda = p.Deaths > 0 ? Math.Round((double)(p.Kills + p.Assists) / p.Deaths, 2) : p.Kills + p.Assists,
                    GoldEarned = p.GoldEarned,
                    TotalDamageDealtToChampions = p.TotalDamageDealtToChampions,
                    TotalDamageTaken = p.TotalDamageTaken,
                    VisionScore = p.VisionScore,
                    Cs = p.Cs,
                    Item0 = p.Item0 ?? 0,
                    Item1 = p.Item1 ?? 0,
                    Item2 = p.Item2 ?? 0,
                    Item3 = p.Item3 ?? 0,
                    Item4 = p.Item4 ?? 0,
                    Item5 = p.Item5 ?? 0,
                    Item6 = p.Item6 ?? 0,
                    Summoner1Id = p.Summoner1Id ?? 0,
                    Summoner2Id = p.Summoner2Id ?? 0,
                    PrimaryRuneId = p.Perk0 ?? 0,
                    SecondaryRunePathId = p.PerkSubStyle ?? 0
                }).ToList()
            })
            .ToList();

        return new MatchDetailDto
        {
            MatchId = match.MatchId,
            GameCreation = match.GameCreation,
            GameDuration = match.GameDuration,
            GameMode = match.GameMode,
            QueueId = match.QueueId,
            GameVersion = match.GameVersion,
            Teams = teams
        };
    }
}
