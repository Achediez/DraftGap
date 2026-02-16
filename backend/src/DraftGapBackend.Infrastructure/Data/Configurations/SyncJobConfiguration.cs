using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for SyncJob entity.
/// Tracks background jobs for synchronizing Riot API data (match history, ranked stats).
/// Enables monitoring, retry logic, and job status tracking.
/// </summary>
public class SyncJobConfiguration : IEntityTypeConfiguration<SyncJob>
{
    public void Configure(EntityTypeBuilder<SyncJob> builder)
    {
        // Table mapping
        builder.ToTable("sync_jobs");

        // Primary key (auto-increment)
        builder.HasKey(e => e.JobId);

        // Performance indexes for job queue queries
        builder.HasIndex(e => new { e.Status, e.CreatedAt })
               .HasDatabaseName("idx_status_created");

        builder.HasIndex(e => new { e.Puuid, e.Status })
               .HasDatabaseName("idx_puuid_status");

        // Required columns
        builder.Property(e => e.Puuid)
               .IsRequired()
               .HasMaxLength(78);

        builder.Property(e => e.JobType)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.Status)
               .IsRequired()
               .HasMaxLength(20);

        // Error message can be long text
        builder.Property(e => e.ErrorMessage)
               .HasColumnType("text");

        // Foreign key to Player
        builder.HasOne(e => e.Player)
               .WithMany()
               .HasForeignKey(e => e.Puuid)
               .HasPrincipalKey(p => p.Puuid)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
