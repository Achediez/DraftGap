using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace DraftGapBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for User entity.
/// Defines authentication table mapping, GUID conversion, and unique constraints.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table mapping
        builder.ToTable("users");

        // Primary key
        builder.HasKey(e => e.UserId);

        // Guid to CHAR(36) conversion
        builder.Property(e => e.UserId)
               .HasColumnName("user_id")
               .HasColumnType("char(36)")
               .HasConversion(
                   v => v.ToString("D"),
                   v => Guid.Parse(v))
               .IsRequired();

        // Column configurations
        builder.Property(e => e.Email)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(e => e.PasswordHash)
               .IsRequired()
               .HasMaxLength(255);

        // Unique indexes
        builder.HasIndex(e => e.Email)
               .IsUnique()
               .HasDatabaseName("uq_email");

        builder.HasIndex(e => e.RiotId)
               .IsUnique()
               .HasDatabaseName("uq_riot_id");

        builder.HasIndex(e => e.RiotPuuid)
               .IsUnique()
               .HasDatabaseName("uq_riot_puuid");

        builder.HasIndex(e => e.LastSync)
               .HasDatabaseName("idx_last_sync");
    }
}
