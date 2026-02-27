using DraftGapBackend.Application.Dtos.Dashboard;
using System;
using System.Collections.Generic;

namespace DraftGapBackend.Application.Dtos.Users;

/// <summary>
/// DTO con detalles agregados de usuario buscado por Riot ID.
/// Combina datos de:
/// - users: información básica del usuario
/// - players: summoner info (si existe)
/// - player_ranked_stats: ranked overview (si existe)
/// - match_participants: partidas recientes y top champions
/// Usado en: GET /api/users/by-riot-id/{riotId}
/// </summary>
public class UserDetailsByRiotIdDto
{
    /// <summary>
    /// ID único del usuario en la plataforma
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Email del usuario
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Riot ID en formato GameName#TAG
    /// </summary>
    public string RiotId { get; set; } = string.Empty;
    
    /// <summary>
    /// Región del usuario (euw1, na1, etc.)
    /// </summary>
    public string? Region { get; set; }
    
    /// <summary>
    /// Última fecha de sincronización de datos
    /// </summary>
    public DateTime? LastSync { get; set; }
    
    /// <summary>
    /// Información del summoner (null si no está vinculado)
    /// </summary>
    public UserSummonerInfoDto? Summoner { get; set; }
    
    /// <summary>
    /// Resumen de ranked (Solo/Duo y Flex)
    /// Null si el usuario no ha jugado ranked
    /// </summary>
    public RankedOverviewDto? RankedOverview { get; set; }
    
    /// <summary>
    /// Últimas 10 partidas jugadas
    /// Array vacío si no hay partidas
    /// </summary>
    public List<RecentMatchDto> RecentMatches { get; set; } = [];
    
    /// <summary>
    /// Top 5 campeones más jugados (basado en últimas 50 partidas)
    /// Array vacío si no hay datos suficientes
    /// </summary>
    public List<TopChampionDto> TopChampions { get; set; } = [];
}

/// <summary>
/// Información básica del summoner
/// </summary>
public class UserSummonerInfoDto
{
    public string Puuid { get; set; } = string.Empty;
    public string SummonerName { get; set; } = string.Empty;
    public int ProfileIconId { get; set; }
    public long SummonerLevel { get; set; }
}
