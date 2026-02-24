using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for Item entity.
/// Defines table mapping, column constraints, and indexes for static item data.
/// </summary>
public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        // Table mapping
        builder.ToTable("items");

        // Primary key
        builder.HasKey(e => e.item_id);

        // Column configurations
        builder.Property(e => e.item_name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.description)
               .HasColumnType("text");

        builder.Property(e => e.gold_cost)
               .HasDefaultValue(0);

        builder.Property(e => e.image_url)
               .HasMaxLength(255);

        builder.Property(e => e.version)
               .IsRequired()
               .HasMaxLength(20);

        // Indexes
        builder.HasIndex(e => e.item_name)
               .HasDatabaseName("idx_item_name");
    }
}
