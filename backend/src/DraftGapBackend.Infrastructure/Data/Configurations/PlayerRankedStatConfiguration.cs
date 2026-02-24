using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for PlayerRankedStat entity.
/// Stores ranked queue statistics (Solo/Duo, Flex) for players.
/// Each player can have multiple ranked stat records (one per queue type).
/// </summary>
public class PlayerRankedStatConfiguration : IEntityTypeConfiguration<PlayerRankedStat>
{
    public void Configure(EntityTypeBuilder<PlayerRankedStat> builder)
    {
        // Table mapping
        builder.ToTable("player_ranked_stats");

        // Primary key (auto-increment)
        builder.HasKey(e => e.RankedStatId);

        // Unique constraint: one row per player per queue type
        builder.HasIndex(e => new { e.Puuid, e.QueueType })
               .IsUnique()
               .HasDatabaseName("uq_puuid_queue");

        // Performance index for leaderboard queries
        builder.HasIndex(e => new { e.Tier, e.Rank })
               .HasDatabaseName("idx_tier_rank");

        // Required columns
        builder.Property(e => e.Puuid)
               .IsRequired()
               .HasMaxLength(78);

        builder.Property(e => e.QueueType)
               .IsRequired()
               .HasMaxLength(50);

        // Optional rank columns (null for unranked players)
        builder.Property(e => e.Tier)
               .HasMaxLength(20);

        builder.Property(e => e.Rank)
               .HasMaxLength(5);

        // Foreign key to Player
        builder.HasOne(e => e.Player)
               .WithMany(p => p.RankedStats)
               .HasForeignKey(e => e.Puuid)
               .HasPrincipalKey(p => p.Puuid)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
