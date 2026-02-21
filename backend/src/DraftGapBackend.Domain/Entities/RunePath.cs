using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// Represents a League of Legends rune path (also called rune style or tree).
/// There are 5 paths: Precision, Domination, Sorcery, Resolve, and Inspiration.
/// Each path contains 4 slots: 1 keystone slot and 3 minor rune slots.
/// This entity is the parent of the Rune entity in a 1:N relationship.
/// Data is synced once on application startup from Riot's CDN.
/// </summary>
[Table("rune_paths")]
public class RunePath
{
    /// <summary>
    /// Riot's unique numeric identifier for the rune path.
    /// Example: 8000 for Precision, 8100 for Domination, 8200 for Sorcery.
    /// </summary>
    [Key]
    [Column("path_id")]
    public int path_id { get; set; }

    /// <summary>
    /// Internal key identifier for the rune path.
    /// Example: "Precision", "Domination", "Sorcery", "Resolve", "Inspiration".
    /// Used for image URL construction and API references.
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("path_key")]
    public string path_key { get; set; } = string.Empty;

    /// <summary>
    /// Localized display name of the rune path.
    /// Example (es_ES): "Precisión", "Dominación", "Hechicería".
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("path_name")]
    public string path_name { get; set; } = string.Empty;

    /// <summary>
    /// Full URL to the rune path's icon image from Data Dragon CDN.
    /// Example: "https://ddragon.leagueoflegends.com/cdn/img/perk-images/Styles/Precision.png".
    /// Used for displaying path icons in the rune selection UI.
    /// </summary>
    [MaxLength(255)]
    [Column("image_url")]
    public string? image_url { get; set; }

    /// <summary>
    /// Game patch version when this rune path data was last synced.
    /// Example: "16.3.1".
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("version")]
    public string version { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to all runes belonging to this path.
    /// Populated when querying with Include() in Entity Framework.
    /// </summary>
    public ICollection<Rune> Runes { get; set; } = new List<Rune>();
}
