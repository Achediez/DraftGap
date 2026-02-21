using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for Player entity.
/// Players store Riot summoner data and link to match participation records.
/// PUUID is the permanent unique identifier used by Riot across regions.
/// </summary>
public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        // Table mapping
        builder.ToTable("players");

        // Primary key (auto-increment)
        builder.HasKey(e => e.PlayerId);

        // Alternate key - PUUID is globally unique across Riot
        builder.HasAlternateKey(e => e.Puuid);

        // Column configurations
        builder.Property(e => e.Puuid)
               .IsRequired()
               .HasMaxLength(78);

        builder.Property(e => e.SummonerId)
               .HasMaxLength(63);

        builder.Property(e => e.SummonerName)
               .HasMaxLength(100);

        builder.Property(e => e.Region)
               .IsRequired()
               .HasMaxLength(10);

        // Performance indexes
        builder.HasIndex(e => e.SummonerId)
               .HasDatabaseName("idx_summoner_id");

        builder.HasIndex(e => e.Region)
               .HasDatabaseName("idx_region");
    }
}
