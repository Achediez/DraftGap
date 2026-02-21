using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for Rune entity.
/// Defines table mapping, column constraints, and foreign key to RunePath.
/// </summary>
public class RuneConfiguration : IEntityTypeConfiguration<Rune>
{
    public void Configure(EntityTypeBuilder<Rune> builder)
    {
        // Table mapping
        builder.ToTable("runes");

        // Primary key
        builder.HasKey(e => e.rune_id);

        // Column configurations
        builder.Property(e => e.path_id)
               .IsRequired();

        builder.Property(e => e.slot)
               .IsRequired();

        builder.Property(e => e.rune_key)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.rune_name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.short_desc)
               .HasColumnType("text");

        builder.Property(e => e.image_url)
               .HasMaxLength(255);

        builder.Property(e => e.version)
               .IsRequired()
               .HasMaxLength(20);

        // Composite index for efficient slot-based queries
        builder.HasIndex(e => new { e.path_id, e.slot })
               .HasDatabaseName("idx_path_slot");
    }
}
