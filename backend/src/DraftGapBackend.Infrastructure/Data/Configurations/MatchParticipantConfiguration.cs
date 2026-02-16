using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for MatchParticipant entity.
/// Stores individual player performance data within a match (kills, deaths, assists, items, etc.).
/// Links Match and Player entities through foreign keys.
/// </summary>
public class MatchParticipantConfiguration : IEntityTypeConfiguration<MatchParticipant>
{
    public void Configure(EntityTypeBuilder<MatchParticipant> builder)
    {
        // Table mapping
        builder.ToTable("match_participants");

        // Primary key (auto-increment)
        builder.HasKey(e => e.ParticipantId);

        // Unique constraint: player can only appear once per match
        builder.HasIndex(e => new { e.MatchId, e.Puuid })
               .IsUnique()
               .HasDatabaseName("uq_match_puuid");

        // Performance indexes for analytics queries
        builder.HasIndex(e => new { e.Puuid, e.Win, e.ChampionId })
               .HasDatabaseName("idx_puuid_analytics");

        builder.HasIndex(e => new { e.ChampionId, e.TeamPosition })
               .HasDatabaseName("idx_champion_role");

        builder.HasIndex(e => new { e.ChampionId, e.TeamPosition, e.Win })
               .HasDatabaseName("idx_champion_role_win");

        // Required columns
        builder.Property(e => e.MatchId)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.Puuid)
               .IsRequired()
               .HasMaxLength(78);

        builder.Property(e => e.ChampionName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.TeamPosition)
               .IsRequired()
               .HasMaxLength(20);

        // Foreign key relationships
        // Match → MatchParticipant (1:N)
        builder.HasOne(e => e.Match)
               .WithMany(m => m.Participants)
               .HasForeignKey(e => e.MatchId)
               .OnDelete(DeleteBehavior.Cascade);

        // Player → MatchParticipant (1:N)
        // Uses PUUID as foreign key (not PlayerId)
        builder.HasOne(e => e.Player)
               .WithMany(p => p.MatchParticipants)
               .HasForeignKey(e => e.Puuid)
               .HasPrincipalKey(p => p.Puuid)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
