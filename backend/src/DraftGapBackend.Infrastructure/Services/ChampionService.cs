using DraftGapBackend.Application.Champions;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de campeones y sus estadísticas.
/// Responsabilidades:
/// - Proveer datos estáticos de campeones (Data Dragon)
/// - Calcular estadísticas de campeones por usuario
/// - Agregar datos de rendimiento: games, winrate, KDA por champion
/// Usa datos de: champions (estáticos) y match_participants (jugados)
/// </summary>
public class ChampionService : IChampionService
{
    private readonly IChampionRepository _championRepository;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChampionService> _logger;

    public ChampionService(
        IChampionRepository championRepository,
        IUserRepository userRepository,
        ApplicationDbContext context,
        ILogger<ChampionService> logger)
    {
        _championRepository = championRepository;
        _userRepository = userRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<List<ChampionDto>> GetAllChampionsAsync(CancellationToken cancellationToken = default)
    {
        var champions = await _championRepository.GetAllAsync(cancellationToken);
        return champions.Select(c => new ChampionDto
        {
            ChampionId = c.champion_id,
            ChampionKey = c.champion_key,
            ChampionName = c.champion_name,
            Title = c.title,
            ImageUrl = c.image_url,
            Version = c.version
        }).ToList();
    }

    public async Task<ChampionDto?> GetChampionByIdAsync(int championId, CancellationToken cancellationToken = default)
    {
        var champion = await _championRepository.GetByIdAsync(championId, cancellationToken);
        if (champion == null)
            return null;

        return new ChampionDto
        {
            ChampionId = champion.champion_id,
            ChampionKey = champion.champion_key,
            ChampionName = champion.champion_name,
            Title = champion.title,
            ImageUrl = champion.image_url,
            Version = champion.version
        };
    }

    /// <summary>
    /// Obtiene estadísticas de todos los campeones jugados por el usuario.
    /// Proceso:
    /// 1. Agrupa todas las participaciones por ChampionId
    /// 2. Calcula totales: games, wins, K/D/A
    /// 3. Combina con datos estáticos (imageUrl) de la tabla champions
    /// 4. Ordena por cantidad de partidas jugadas (descendente)
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de estadísticas por campeón, ordenada por games played</returns>
    public async Task<List<ChampionStatsDto>> GetUserChampionStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RiotPuuid))
            return [];

        var puuid = user.RiotPuuid;

        // Agregar todas las participaciones por champion
        // GroupBy realiza la agregación en SQL para mejor rendimiento
        var championStats = await _context.MatchParticipants
            .Where(p => p.Puuid == puuid)
            .GroupBy(p => new { p.ChampionId, p.ChampionName })
            .Select(g => new
            {
                ChampionId = g.Key.ChampionId,
                ChampionName = g.Key.ChampionName,
                GamesPlayed = g.Count(),
                Wins = g.Count(p => p.Win),
                TotalKills = g.Sum(p => p.Kills),
                TotalDeaths = g.Sum(p => p.Deaths),
                TotalAssists = g.Sum(p => p.Assists)
            })
            .ToListAsync(cancellationToken);

        // Cargar datos estáticos de campeones para obtener imageUrl
        var champions = await _championRepository.GetAllAsync(cancellationToken);
        var championDict = champions.ToDictionary(c => c.champion_id, c => c);

        // Mapear a DTO con cálculos de winrate y KDA
        return championStats.Select(cs => new ChampionStatsDto
        {
            ChampionId = cs.ChampionId,
            ChampionName = cs.ChampionName,
            ImageUrl = championDict.TryGetValue(cs.ChampionId, out var champ) ? champ.image_url : null,
            GamesPlayed = cs.GamesPlayed,
            Wins = cs.Wins,
            Losses = cs.GamesPlayed - cs.Wins,
            Winrate = Math.Round((double)cs.Wins / cs.GamesPlayed * 100, 2),
            AvgKills = Math.Round((double)cs.TotalKills / cs.GamesPlayed, 2),
            AvgDeaths = Math.Round((double)cs.TotalDeaths / cs.GamesPlayed, 2),
            AvgAssists = Math.Round((double)cs.TotalAssists / cs.GamesPlayed, 2),
            AvgKda = cs.TotalDeaths > 0 ? Math.Round((double)(cs.TotalKills + cs.TotalAssists) / cs.TotalDeaths, 2) : cs.TotalKills + cs.TotalAssists,
            TotalKills = cs.TotalKills,
            TotalDeaths = cs.TotalDeaths,
            TotalAssists = cs.TotalAssists
        })
        .OrderByDescending(c => c.GamesPlayed)
        .ToList();
    }
}
