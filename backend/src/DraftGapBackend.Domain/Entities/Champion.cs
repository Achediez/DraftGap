using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// Represents a League of Legends champion with static data from Data Dragon.
/// This entity stores champion metadata for display and analytics purposes.
/// Data is synced once on application startup from Riot's CDN.
/// </summary>
[Table("champions")]
public class Champion
{
    /// <summary>
    /// Riot's unique numeric identifier for the champion.
    /// Example: 266 for Aatrox, 103 for Ahri.
    /// Used as primary key and referenced in match_participants table.
    /// </summary>
    [Key]
    [Column("champion_id")]
    public int champion_id { get; set; }

    /// <summary>
    /// Champion's internal key name used in Riot APIs.
    /// Example: "Aatrox", "Ahri", "MonkeyKing" (for Wukong).
    /// This is the identifier used in Data Dragon URLs.
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("champion_key")]
    public string champion_key { get; set; } = string.Empty;

    /// <summary>
    /// Champion's localized display name.
    /// Example (es_ES): "Aatrox", "Ahri", "Wukong".
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("champion_name")]
    public string champion_name { get; set; } = string.Empty;

    /// <summary>
    /// Champion's localized title or tagline.
    /// Example (es_ES): "la Espada de los Oscuros", "la Mujer Zorro de Nueve Colas".
    /// </summary>
    [MaxLength(100)]
    [Column("title")]
    public string? title { get; set; }

    /// <summary>
    /// Full URL to the champion's square portrait image from Data Dragon CDN.
    /// Example: "https://ddragon.leagueoflegends.com/cdn/16.3.1/img/champion/Aatrox.png".
    /// Used for displaying champion icons in the UI.
    /// </summary>
    [MaxLength(255)]
    [Column("image_url")]
    public string? image_url { get; set; }

    /// <summary>
    /// Game patch version when this champion data was last synced.
    /// Example: "16.3.1", "14.21.1".
    /// Used to determine if re-sync is needed after patch updates.
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("version")]
    public string version { get; set; } = string.Empty;
}
