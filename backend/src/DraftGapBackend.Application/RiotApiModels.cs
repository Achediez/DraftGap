using System;
using System.Collections.Generic;

namespace DraftGapBackend.Infrastructure.Riot;

/// <summary>
/// Riot Account DTO (from account-v1 API)
/// </summary>
public class RiotAccountDto
{
    public string Puuid { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string TagLine { get; set; } = string.Empty;
}

/// <summary>
/// Summoner DTO (from summoner-v4 API)
/// </summary>
public class SummonerDto
{
    public string Id { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string Puuid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ProfileIconId { get; set; }
    public long RevisionDate { get; set; }
    public int SummonerLevel { get; set; }
}

/// <summary>
/// Ranked Stats DTO (from league-v4 API)
/// </summary>
public class RankedStatsDto
{
    public string LeagueId { get; set; } = string.Empty;
    public string QueueType { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string SummonerId { get; set; } = string.Empty;
    public int LeaguePoints { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public bool Veteran { get; set; }
    public bool Inactive { get; set; }
    public bool FreshBlood { get; set; }
    public bool HotStreak { get; set; }
}

/// <summary>
/// Match DTO (from match-v5 API)
/// </summary>
public class MatchDto
{
    public MatchMetadataDto Metadata { get; set; } = new();
    public MatchInfoDto Info { get; set; } = new();
}

public class MatchMetadataDto
{
    public string DataVersion { get; set; } = string.Empty;
    public string MatchId { get; set; } = string.Empty;
    public List<string> Participants { get; set; } = new();
}

public class MatchInfoDto
{
    public long GameCreation { get; set; }
    public int GameDuration { get; set; }
    public long GameEndTimestamp { get; set; }
    public long GameId { get; set; }
    public string GameMode { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public long GameStartTimestamp { get; set; }
    public string GameType { get; set; } = string.Empty;
    public string GameVersion { get; set; } = string.Empty;
    public int MapId { get; set; }
    public List<ParticipantDto> Participants { get; set; } = new();
    public string PlatformId { get; set; } = string.Empty;
    public int QueueId { get; set; }
    public List<TeamDto> Teams { get; set; } = new();
    public string TournamentCode { get; set; } = string.Empty;
}

public class ParticipantDto
{
    public int Assists { get; set; }
    public int BaronKills { get; set; }
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = string.Empty;
    public int ChampLevel { get; set; }
    public int Deaths { get; set; }
    public int DoubleKills { get; set; }
    public int DragonKills { get; set; }
    public bool FirstBloodKill { get; set; }
    public int GoldEarned { get; set; }
    public int GoldSpent { get; set; }
    public string IndividualPosition { get; set; } = string.Empty;
    public int Item0 { get; set; }
    public int Item1 { get; set; }
    public int Item2 { get; set; }
    public int Item3 { get; set; }
    public int Item4 { get; set; }
    public int Item5 { get; set; }
    public int Item6 { get; set; }
    public int Kills { get; set; }
    public int PentaKills { get; set; }
    public int QuadraKills { get; set; }
    public string Puuid { get; set; } = string.Empty;
    public string RiotIdGameName { get; set; } = string.Empty;
    public string RiotIdTagline { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Summoner1Id { get; set; }
    public int Summoner2Id { get; set; }
    public string SummonerName { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public string TeamPosition { get; set; } = string.Empty;
    public int TotalDamageDealt { get; set; }
    public int TotalDamageDealtToChampions { get; set; }
    public int TotalDamageTaken { get; set; }
    public int TotalMinionsKilled { get; set; }
    public int NeutralMinionsKilled { get; set; }
    public int TripleKills { get; set; }
    public int VisionScore { get; set; }
    public bool Win { get; set; }
    public PerksDto Perks { get; set; } = new();
}

public class PerksDto
{
    public PerkStatsDto StatPerks { get; set; } = new();
    public List<PerkStyleDto> Styles { get; set; } = new();
}

public class PerkStatsDto
{
    public int Defense { get; set; }
    public int Flex { get; set; }
    public int Offense { get; set; }
}

public class PerkStyleDto
{
    public string Description { get; set; } = string.Empty;
    public List<PerkStyleSelectionDto> Selections { get; set; } = new();
    public int Style { get; set; }
}

public class PerkStyleSelectionDto
{
    public int Perk { get; set; }
    public int Var1 { get; set; }
    public int Var2 { get; set; }
    public int Var3 { get; set; }
}

public class TeamDto
{
    public List<BanDto> Bans { get; set; } = new();
    public ObjectivesDto Objectives { get; set; } = new();
    public int TeamId { get; set; }
    public bool Win { get; set; }
}

public class BanDto
{
    public int ChampionId { get; set; }
    public int PickTurn { get; set; }
}

public class ObjectivesDto
{
    public ObjectiveDto Baron { get; set; } = new();
    public ObjectiveDto Champion { get; set; } = new();
    public ObjectiveDto Dragon { get; set; } = new();
    public ObjectiveDto Horde { get; set; } = new();
    public ObjectiveDto Inhibitor { get; set; } = new();
    public ObjectiveDto RiftHerald { get; set; } = new();
    public ObjectiveDto Tower { get; set; } = new();
}

public class ObjectiveDto
{
    public bool First { get; set; }
    public int Kills { get; set; }
}

/// <summary>
/// Rate limit status
/// </summary>
public class RateLimitStatus
{
    public int RequestsPerSecond { get; set; }
    public int RequestsPer2Minutes { get; set; }
    public int RemainingPerSecond { get; set; }
    public int RemainingPer2Minutes { get; set; }
    public DateTime ResetTimePerSecond { get; set; }
    public DateTime ResetTimePer2Minutes { get; set; }
}
