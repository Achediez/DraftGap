using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DraftGapBackend.Domain.Entities;

/// <summary>
/// Background sync job tracking
/// </summary>
[Table("sync_jobs")]
public class SyncJob
{
    [Key]
    [Column("job_id")]
    public long JobId { get; set; }

    [Required]
    [MaxLength(78)]
    [Column("puuid")]
    public string Puuid { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("job_type")]
    public string JobType { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "PENDING";

    [Column("matches_processed")]
    public int MatchesProcessed { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("Puuid")]
    public virtual Player Player { get; set; } = null!;

    // Computed property
    [NotMapped]
    public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : null;
}
