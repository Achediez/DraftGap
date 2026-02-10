using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// Riot API player/summoner data
/// </summary>
[Table("players")]
public class Player
{
    [Key]
    [Column("player_id")]
    public int PlayerId { get; set; }

    [Required]
    [MaxLength(78)]
    [Column("puuid")]
    public string Puuid { get; set; } = string.Empty;

    [MaxLength(63)]
    [Column("summoner_id")]
    public string? SummonerId { get; set; }

    [MaxLength(100)]
    [Column("summoner_name")]
    public string? SummonerName { get; set; }

    [Column("profile_icon_id")]
    public int? ProfileIconId { get; set; }

    [Column("summoner_level")]
    public int? SummonerLevel { get; set; }

    [Required]
    [MaxLength(10)]
    [Column("region")]
    public string Region { get; set; } = string.Empty;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<MatchParticipant> MatchParticipants { get; set; } = new List<MatchParticipant>();
    public virtual ICollection<PlayerRankedStat> RankedStats { get; set; } = new List<PlayerRankedStat>();
}
