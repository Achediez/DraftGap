using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for Champion entity.
/// Defines table mapping, column constraints, and indexes for static champion data.
/// </summary>
public class ChampionConfiguration : IEntityTypeConfiguration<Champion>
{
    public void Configure(EntityTypeBuilder<Champion> builder)
    {
        // Table mapping
        builder.ToTable("champions");

        // Primary key
        builder.HasKey(e => e.champion_id);

        // Column configurations
        builder.Property(e => e.champion_key)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.champion_name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.title)
               .HasMaxLength(100);

        builder.Property(e => e.image_url)
               .HasMaxLength(255);

        builder.Property(e => e.version)
               .IsRequired()
               .HasMaxLength(20);

        // Indexes
        builder.HasIndex(e => e.champion_key)
               .HasDatabaseName("idx_champion_key");
    }
}
