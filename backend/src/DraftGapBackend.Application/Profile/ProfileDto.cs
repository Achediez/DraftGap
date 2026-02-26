using System;

namespace DraftGapBackend.Application.Profile;

/// <summary>
/// User profile information response
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
/// Request to update user profile
/// </summary>
public class UpdateProfileRequest
{
    public string? RiotId { get; set; }
    public string? Region { get; set; }
}
