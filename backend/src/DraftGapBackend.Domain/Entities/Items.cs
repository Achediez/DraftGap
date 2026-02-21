using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// Represents a League of Legends item with static data from Data Dragon.
/// Items are purchasable equipment that provide stats and abilities to champions.
/// This entity stores item metadata for display in match builds and analytics.
/// Data is synced once on application startup from Riot's CDN.
/// </summary>
[Table("items")]
public class Item
{
    /// <summary>
    /// Riot's unique numeric identifier for the item.
    /// Example: 3078 for Trinity Force, 3157 for Zhonya's Hourglass.
    /// Used as primary key and referenced in match participant item builds.
    /// </summary>
    [Key]
    [Column("item_id")]
    public int item_id { get; set; }

    /// <summary>
    /// Item's localized display name.
    /// Example (es_ES): "Fuerza de trinidad", "Reloj de arena de Zhonya".
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("item_name")]
    public string item_name { get; set; } = string.Empty;

    /// <summary>
    /// Localized item description including passive/active effects.
    /// Example: "Otorga vida, maná, velocidad de ataque y daño...".
    /// Can be long text with HTML-like tags for formatting.
    /// </summary>
    [Column("description")]
    public string? description { get; set; }

    /// <summary>
    /// Total gold cost to purchase the item.
    /// Example: 3333 for Trinity Force.
    /// Used to calculate total build cost in match analysis.
    /// </summary>
    [Column("gold_cost")]
    public int? gold_cost { get; set; }

    /// <summary>
    /// Full URL to the item's square icon image from Data Dragon CDN.
    /// Example: "https://ddragon.leagueoflegends.com/cdn/16.3.1/img/item/3078.png".
    /// Used for displaying item icons in UI.
    /// </summary>
    [MaxLength(255)]
    [Column("image_url")]
    public string? image_url { get; set; }

    /// <summary>
    /// Game patch version when this item data was last synced.
    /// Example: "16.3.1", "14.21.1".
    /// Used to determine if re-sync is needed after patch updates.
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("version")]
    public string version { get; set; } = string.Empty;
}
