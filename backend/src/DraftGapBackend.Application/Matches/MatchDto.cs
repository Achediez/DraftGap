using System;
using System.Collections.Generic;

namespace DraftGapBackend.Application.Matches;

/// <summary>
/// Filtros opcionales para el historial de partidas.
/// Todos los campos son opcionales:
/// - championName: Filtrar por campeón específico
/// - teamPosition: TOP, JUNGLE, MIDDLE, BOTTOM, UTILITY
/// - win: true=victorias, false=derrotas, null=todas
/// - queueId: 420=Ranked Solo, 440=Flex, 0=todas
/// - startDate/endDate: Rango de fechas
/// Aplicados en memoria después de la paginación.
/// </summary>
public class MatchFilterRequest
{
    public string? ChampionName { get; set; }
    public string? TeamPosition { get; set; }
    public bool? Win { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int QueueId { get; set; } = 0; // 0 = all queues
}

/// <summary>
/// DTO con detalles completos de una partida específica.
/// Incluye:
/// - Info general: duración, modo, versión del juego
/// - Resultado por equipo (team100Won/team200Won)
/// - Todos los participantes (10 jugadores) con stats completas
/// Usado en: GET /api/matches/{matchId}
/// </summary>
public class MatchDetailDto
/// </summary>
public class MatchDetailDto
{
    public string MatchId { get; set; } = string.Empty;
    public long GameCreation { get; set; }
    public int GameDuration { get; set; }
    public string GameMode { get; set; } = string.Empty;
    public int QueueId { get; set; }
    public string GameVersion { get; set; } = string.Empty;
    public List<TeamDto> Teams { get; set; } = [];
}

/// <summary>
/// Team information in a match
/// </summary>
public class TeamDto
{
    public int TeamId { get; set; }
    public bool Win { get; set; }
    public List<ParticipantDto> Participants { get; set; } = [];
}

/// <summary>
/// Participant information in a match
/// </summary>
public class ParticipantDto
{
    public string Puuid { get; set; } = string.Empty;
    public string? RiotIdGameName { get; set; }
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = string.Empty;
    public int ChampLevel { get; set; }
    public string TeamPosition { get; set; } = string.Empty;
    public bool Win { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public double Kda { get; set; }
    public int GoldEarned { get; set; }
    public int TotalDamageDealtToChampions { get; set; }
    public int TotalDamageTaken { get; set; }
    public int VisionScore { get; set; }
    public int Cs { get; set; }
    public int Item0 { get; set; }
    public int Item1 { get; set; }
    public int Item2 { get; set; }
    public int Item3 { get; set; }
    public int Item4 { get; set; }
    public int Item5 { get; set; }
    public int Item6 { get; set; }
    public int Summoner1Id { get; set; }
    public int Summoner2Id { get; set; }
    public int PrimaryRuneId { get; set; }
    public int SecondaryRunePathId { get; set; }
}

/// <summary>
/// Simplified match list item
/// </summary>
public class MatchListItemDto
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
    public int QueueId { get; set; }
}
