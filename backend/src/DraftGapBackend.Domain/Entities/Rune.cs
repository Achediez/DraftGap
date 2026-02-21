using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// Represents an individual League of Legends rune within a rune path.
/// Each rune belongs to a specific path and occupies a slot position.
/// Slot 0 contains keystones (e.g., Conqueror, Electrocute).
/// Slots 1-3 contain minor runes (e.g., Triumph, Legend: Alacrity, Coup de Grace).
/// Data is synced once on application startup from Riot's CDN.
/// </summary>
[Table("runes")]
public class Rune
{
    /// <summary>
    /// Riot's unique numeric identifier for the rune.
    /// Example: 8008 for Lethal Tempo, 8021 for Fleet Footwork, 8010 for Conqueror.
    /// Referenced in match_participants.perk0 through perk5 columns.
    /// </summary>
    [Key]
    [Column("rune_id")]
    public int rune_id { get; set; }

    /// <summary>
    /// Foreign key linking this rune to its parent rune path.
    /// Example: Conqueror (8010) belongs to Precision (8000).
    /// </summary>
    [Required]
    [Column("path_id")]
    public int path_id { get; set; }

    /// <summary>
    /// Slot position within the rune path.
    /// 0 = Keystone slot (most powerful rune, defines playstyle).
    /// 1, 2, 3 = Minor rune slots (secondary bonuses).
    /// </summary>
    [Required]
    [Column("slot")]
    public int slot { get; set; }

    /// <summary>
    /// Internal key identifier for the rune.
    /// Example: "Conqueror", "Triumph", "LegendAlacrity".
    /// Used for image URL construction.
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("rune_key")]
    public string rune_key { get; set; } = string.Empty;

    /// <summary>
    /// Localized display name of the rune.
    /// Example (es_ES): "Conquistador", "Triunfo", "Leyenda: Presteza".
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("rune_name")]
    public string rune_name { get; set; } = string.Empty;

    /// <summary>
    /// Short localized description of the rune's effect.
    /// Contains simplified explanation of what the rune does.
    /// Example: "Después de infligir daño, ganas stacks de Conquistador..."
    /// </summary>
    [Column("short_desc")]
    public string? short_desc { get; set; }

    /// <summary>
    /// Full URL to the rune's icon image from Data Dragon CDN.
    /// Example: "https://ddragon.leagueoflegends.com/cdn/img/perk-images/Styles/Precision/Conqueror/Conqueror.png".
    /// Used for displaying rune icons in match summaries.
    /// </summary>
    [MaxLength(255)]
    [Column("image_url")]
    public string? image_url { get; set; }

    /// <summary>
    /// Game patch version when this rune data was last synced.
    /// Example: "16.3.1".
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("version")]
    public string version { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the parent rune path.
    /// Populated when querying with Include() in Entity Framework.
    /// </summary>
    [ForeignKey(nameof(path_id))]
    public RunePath RunePath { get; set; } = null!;
}
