using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// Player performance data for a single participant slot within a match.
/// </summary>
[Table("match_participants")]
public class MatchParticipant
{
    [Key]
    [Column("participant_id")]
    public long ParticipantId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("match_id")]
    public string MatchId { get; set; } = string.Empty;

    [Required]
    [MaxLength(78)]
    [Column("puuid")]
    public string Puuid { get; set; } = string.Empty;

    // Display name as returned by Riot (GameName#TAG format).
    [MaxLength(100)]
    [Column("riot_id_game_name")]
    public string? RiotIdGameName { get; set; }

    [Column("champion_id")]
    public int ChampionId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("champion_name")]
    public string ChampionName { get; set; } = string.Empty;

    // Champion level reached at end of game.
    [Column("champ_level")]
    public int ChampLevel { get; set; }

    [Column("team_id")]
    public int TeamId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("team_position")]
    public string TeamPosition { get; set; } = string.Empty;

    [Column("win")]
    public bool Win { get; set; }

    [Column("kills")]
    public int Kills { get; set; }

    [Column("deaths")]
    public int Deaths { get; set; }

    [Column("assists")]
    public int Assists { get; set; }

    [Column("gold_earned")]
    public int GoldEarned { get; set; }

    [Column("total_damage_dealt")]
    public int TotalDamageDealt { get; set; }

    [Column("total_damage_dealt_to_champions")]
    public int TotalDamageDealtToChampions { get; set; }

    [Column("total_damage_taken")]
    public int TotalDamageTaken { get; set; }

    [Column("vision_score")]
    public int VisionScore { get; set; }

    [Column("cs")]
    public int Cs { get; set; }

    [Column("double_kills")]
    public int DoubleKills { get; set; }

    [Column("triple_kills")]
    public int TripleKills { get; set; }

    [Column("quadra_kills")]
    public int QuadraKills { get; set; }

    [Column("penta_kills")]
    public int PentaKills { get; set; }

    [Column("first_blood")]
    public bool FirstBlood { get; set; }

    [Column("summoner1_id")]
    public int? Summoner1Id { get; set; }

    [Column("summoner2_id")]
    public int? Summoner2Id { get; set; }

    [Column("item0")]
    public int? Item0 { get; set; }

    [Column("item1")]
    public int? Item1 { get; set; }

    [Column("item2")]
    public int? Item2 { get; set; }

    [Column("item3")]
    public int? Item3 { get; set; }

    [Column("item4")]
    public int? Item4 { get; set; }

    [Column("item5")]
    public int? Item5 { get; set; }

    [Column("item6")]
    public int? Item6 { get; set; }

    [Column("perk_primary_style")]
    public int? PerkPrimaryStyle { get; set; }

    [Column("perk_sub_style")]
    public int? PerkSubStyle { get; set; }

    [Column("perk0")]
    public int? Perk0 { get; set; }

    [Column("perk1")]
    public int? Perk1 { get; set; }

    [Column("perk2")]
    public int? Perk2 { get; set; }

    [Column("perk3")]
    public int? Perk3 { get; set; }

    [Column("perk4")]
    public int? Perk4 { get; set; }

    [Column("perk5")]
    public int? Perk5 { get; set; }

    [ForeignKey("MatchId")]
    public virtual Match Match { get; set; } = null!;

    [ForeignKey("Puuid")]
    public virtual Player Player { get; set; } = null!;

    [NotMapped]
    public double KDA => Deaths == 0 ? Kills + Assists : (double)(Kills + Assists) / Deaths;
}
