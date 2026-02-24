using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for Match entity.
/// Stores metadata for League of Legends matches fetched from Riot API.
/// Match IDs follow format: {PLATFORM}_{GAME_ID} (e.g., EUW1_6847231920).
/// </summary>
public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        // Table mapping
        builder.ToTable("matches");

        // Primary key (Riot match ID string)
        builder.HasKey(e => e.MatchId);

        // Required string columns
        builder.Property(e => e.GameMode)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.GameType)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.PlatformId)
               .IsRequired()
               .HasMaxLength(10);

        builder.Property(e => e.GameVersion)
               .IsRequired()
               .HasMaxLength(20);

        // Performance indexes for filtering and sorting matches
        builder.HasIndex(e => e.GameCreation)
               .HasDatabaseName("idx_game_creation");

        builder.HasIndex(e => new { e.QueueId, e.GameCreation })
               .HasDatabaseName("idx_queue_date");

        builder.HasIndex(e => new { e.PlatformId, e.GameCreation })
               .HasDatabaseName("idx_platform_date");
    }
}
