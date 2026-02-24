using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for SummonerSpell entity.
/// Defines table mapping, column constraints, and indexes for static summoner spell data.
/// </summary>
public class SummonerSpellConfiguration : IEntityTypeConfiguration<SummonerSpell>
{
    public void Configure(EntityTypeBuilder<SummonerSpell> builder)
    {
        // Table mapping
        builder.ToTable("summoner_spells");

        // Primary key
        builder.HasKey(e => e.spell_id);

        // Column configurations
        builder.Property(e => e.spell_key)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.spell_name)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.description)
               .HasColumnType("text");

        builder.Property(e => e.cooldown);

        builder.Property(e => e.image_url)
               .HasMaxLength(255);

        builder.Property(e => e.version)
               .IsRequired()
               .HasMaxLength(20);

        // Indexes
        builder.HasIndex(e => e.spell_key)
               .HasDatabaseName("idx_spell_key");
    }
}
