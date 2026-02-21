using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for RunePath entity.
/// Defines table mapping, column constraints, and relationship to Rune entity.
/// </summary>
public class RunePathConfiguration : IEntityTypeConfiguration<RunePath>
{
    public void Configure(EntityTypeBuilder<RunePath> builder)
    {
        // Table mapping
        builder.ToTable("rune_paths");

        // Primary key
        builder.HasKey(e => e.path_id);

        // Column configurations
        builder.Property(e => e.path_key)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.path_name)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.image_url)
               .HasMaxLength(255);

        builder.Property(e => e.version)
               .IsRequired()
               .HasMaxLength(20);

        // Performance index for key lookups
        builder.HasIndex(e => e.path_key)
               .HasDatabaseName("idx_path_key");

        // Relationship: RunePath 1:N Rune
        // Cascade delete removes all runes when a path is deleted
        builder.HasMany(e => e.Runes)
               .WithOne(r => r.RunePath)
               .HasForeignKey(r => r.path_id)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
