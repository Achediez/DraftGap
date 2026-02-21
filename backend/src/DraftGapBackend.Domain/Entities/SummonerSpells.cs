using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// Represents a League of Legends summoner spell with static data from Data Dragon.
/// Summoner spells are abilities players select before the game (Flash, Ignite, Teleport, etc.).
/// This entity stores spell metadata for display in match summaries and analytics.
/// Data is synced once on application startup from Riot's CDN.
/// </summary>
[Table("summoner_spells")]
public class SummonerSpell
{
    /// <summary>
    /// Riot's unique numeric identifier for the summoner spell.
    /// Example: 4 for Flash, 14 for Ignite, 12 for Teleport.
    /// Used as primary key and referenced in match_participants table (summoner1_id, summoner2_id).
    /// </summary>
    [Key]
    [Column("spell_id")]
    public int spell_id { get; set; }

    /// <summary>
    /// Spell's internal key name used in Riot APIs.
    /// Example: "SummonerFlash", "SummonerDot" (Ignite), "SummonerTeleport".
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("spell_key")]
    public string spell_key { get; set; } = string.Empty;

    /// <summary>
    /// Spell's localized display name.
    /// Example (es_ES): "Destello" (Flash), "Inflamar" (Ignite), "Teletransporte" (Teleport).
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("spell_name")]
    public string spell_name { get; set; } = string.Empty;

    /// <summary>
    /// Localized spell description explaining what the spell does.
    /// Example (es_ES): "Teletransporta a tu campeón hacia el cursor".
    /// </summary>
    [Column("description")]
    public string? description { get; set; }

    /// <summary>
    /// Spell cooldown in seconds.
    /// Example: 300 for Flash (5 minutes), 180 for Ignite (3 minutes).
    /// Used to show spell availability timings.
    /// </summary>
    [Column("cooldown")]
    public int? cooldown { get; set; }

    /// <summary>
    /// Full URL to the spell's square icon image from Data Dragon CDN.
    /// Example: "https://ddragon.leagueoflegends.com/cdn/16.3.1/img/spell/SummonerFlash.png".
    /// Used for displaying spell icons in match UI.
    /// </summary>
    [MaxLength(255)]
    [Column("image_url")]
    public string? image_url { get; set; }

    /// <summary>
    /// Game patch version when this spell data was last synced.
    /// Example: "16.3.1".
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("version")]
    public string version { get; set; } = string.Empty;
}
