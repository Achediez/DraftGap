using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// Application user with authentication credentials and Riot account linkage.
/// </summary>
[Table("users")]
public class User
{
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public required string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("password_hash")]
    public required string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("riot_id")]
    public string? RiotId { get; set; }

    [MaxLength(78)]
    [Column("riot_puuid")]
    public string? RiotPuuid { get; set; }

    // Riot platform region (e.g. euw1, na1, kr) — used to route API requests correctly.
    [MaxLength(10)]
    [Column("region")]
    public string? Region { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("last_sync")]
    public DateTime? LastSync { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}
