using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// Ranked statistics per queue type
/// </summary>
[Table("player_ranked_stats")]
public class PlayerRankedStat
{
    [Key]
    [Column("ranked_stat_id")]
    public int RankedStatId { get; set; }

    [Required]
    [MaxLength(78)]
    [Column("puuid")]
    public string Puuid { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("queue_type")]
    public string QueueType { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("tier")]
    public string? Tier { get; set; }

    [MaxLength(5)]
    [Column("rank")]
    public string? Rank { get; set; }

    [Column("league_points")]
    public int? LeaguePoints { get; set; }

    [Column("wins")]
    public int Wins { get; set; }

    [Column("losses")]
    public int Losses { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("Puuid")]
    public virtual Player Player { get; set; } = null!;

    // Computed properties
    [NotMapped]
    public int TotalGames => Wins + Losses;

    [NotMapped]
    public double Winrate => TotalGames > 0 ? (double)Wins / TotalGames * 100 : 0;
}
