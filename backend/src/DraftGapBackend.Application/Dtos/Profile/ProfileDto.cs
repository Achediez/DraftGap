using System;

namespace DraftGapBackend.Application.Dtos.Profile;

/// <summary>
/// DTO de perfil de usuario completo.
/// Combina datos de:
/// - users table: email, riotId, region, lastSync, createdAt
/// - players table: summonerName, level, profileIconId (si existe)
/// Usado en: GET /api/profile
/// </summary>
public class ProfileDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? RiotId { get; set; }
    public string? Region { get; set; }
    public DateTime? LastSync { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    public ProfileSummonerDto? Summoner { get; set; }
}

/// <summary>
/// Summoner information within profile
/// </summary>
public class ProfileSummonerDto
{
    public string Puuid { get; set; } = string.Empty;
    public string? SummonerName { get; set; }
    public int? ProfileIconId { get; set; }
    public int? SummonerLevel { get; set; }
}

/// <summary>
/// Request para actualizar perfil de usuario.
/// Campos opcionales:
/// - riotId: Nuevo Riot ID (validado contra Riot API antes de guardar)
/// - region: Nueva región (debe ser platform ID válido: euw1, na1, etc.)
/// Validado por: UpdateProfileRequestValidator
/// Usado en: PUT /api/profile
/// </summary>
public class UpdateProfileRequest
{
    public string? RiotId { get; set; }
    public string? Region { get; set; }
}
