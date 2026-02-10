using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// League of Legends match data
/// </summary>
[Table("matches")]
public class Match
{
    [Key]
    [MaxLength(50)]
    [Column("match_id")]
    public string MatchId { get; set; } = string.Empty;

    [Column("game_creation")]
    public long GameCreation { get; set; }

    [Column("game_duration")]
    public int GameDuration { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("game_mode")]
    public string GameMode { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("game_type")]
    public string GameType { get; set; } = string.Empty;

    [Column("queue_id")]
    public int QueueId { get; set; }

    [Required]
    [MaxLength(10)]
    [Column("platform_id")]
    public string PlatformId { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("game_version")]
    public string GameVersion { get; set; } = string.Empty;

    [Column("fetched_at")]
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<MatchParticipant> Participants { get; set; } = new List<MatchParticipant>();
}
