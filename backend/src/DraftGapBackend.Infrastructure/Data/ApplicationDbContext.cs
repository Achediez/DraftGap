using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DraftGapBackend.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context
/// Maps C# entities to MySQL tables
/// </summary>
///

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets - represent database tables
    public DbSet<User> Users { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<MatchParticipant> MatchParticipants { get; set; }
    public DbSet<PlayerRankedStat> PlayerRankedStats { get; set; }
    public DbSet<SyncJob> SyncJobs { get; set; }

    /// <summary>
    /// Champion static data from Data Dragon.
    /// Contains metadata for all League of Legends champions.
    /// </summary>
    public DbSet<Champion> Champions { get; set; }

    /// <summary>
    /// Item static data from Data Dragon.
    /// Contains metadata for all purchasable items in League of Legends.
    /// </summary>
    public DbSet<Item> Items { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ====================
        // User Entity Configuration
        // ====================
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasColumnType("char(36)")
                .HasConversion(
                    v => v.ToString("D"),
                    v => Guid.Parse(v))  // Use Guid.Parse instead of ParseExact
                .IsRequired();

            entity.Property(e => e.Email).IsRequired();

            entity.Property(e => e.PasswordHash).IsRequired();

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.RiotId).IsUnique();
            entity.HasIndex(e => e.RiotPuuid).IsUnique();
            entity.HasIndex(e => e.LastSync);
        });

        // ====================
        // Player Entity Configuration
        // ====================
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.PlayerId);
            entity.HasAlternateKey(e => e.Puuid); // Puuid is also unique
            entity.HasIndex(e => e.SummonerId);
            entity.HasIndex(e => e.Region);

            entity.Property(e => e.Puuid).IsRequired();
            entity.Property(e => e.Region).IsRequired();
        });

        // ====================
        // Match Entity Configuration
        // ====================
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.MatchId);
            entity.HasIndex(e => e.GameCreation);
            entity.HasIndex(e => new { e.QueueId, e.GameCreation });
            entity.HasIndex(e => new { e.PlatformId, e.GameCreation });

            entity.Property(e => e.GameMode).IsRequired();
            entity.Property(e => e.GameType).IsRequired();
            entity.Property(e => e.PlatformId).IsRequired();
            entity.Property(e => e.GameVersion).IsRequired();
        });

        // ====================
        // MatchParticipant Entity Configuration
        // ====================
        modelBuilder.Entity<MatchParticipant>(entity =>
        {
            entity.HasKey(e => e.ParticipantId);

            // Composite unique index (player can only appear once per match)
            entity.HasIndex(e => new { e.MatchId, e.Puuid }).IsUnique();

            // Performance indexes for analytics queries
            entity.HasIndex(e => new { e.Puuid, e.Win, e.ChampionId });
            entity.HasIndex(e => new { e.ChampionId, e.TeamPosition });
            entity.HasIndex(e => new { e.ChampionId, e.TeamPosition, e.Win });

            // Foreign key to Match with cascade delete
            entity.HasOne(e => e.Match)
                .WithMany(m => m.Participants)
                .HasForeignKey(e => e.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign key to Player (using Puuid) with cascade delete
            entity.HasOne(e => e.Player)
                .WithMany(p => p.MatchParticipants)
                .HasForeignKey(e => e.Puuid)
                .HasPrincipalKey(p => p.Puuid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.MatchId).IsRequired();
            entity.Property(e => e.Puuid).IsRequired();
            entity.Property(e => e.ChampionName).IsRequired();
            entity.Property(e => e.TeamPosition).IsRequired();
        });

        // ====================
        // PlayerRankedStat Entity Configuration
        // ====================
        modelBuilder.Entity<PlayerRankedStat>(entity =>
        {
            entity.HasKey(e => e.RankedStatId);

            // Unique constraint: one row per player per queue type
            entity.HasIndex(e => new { e.Puuid, e.QueueType }).IsUnique();
            entity.HasIndex(e => new { e.Tier, e.Rank });

            // Foreign key to Player
            entity.HasOne(e => e.Player)
                .WithMany(p => p.RankedStats)
                .HasForeignKey(e => e.Puuid)
                .HasPrincipalKey(p => p.Puuid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Puuid).IsRequired();
            entity.Property(e => e.QueueType).IsRequired();
        });

        // ====================
        // SyncJob Entity Configuration
        // ====================
        modelBuilder.Entity<SyncJob>(entity =>
        {
            entity.HasKey(e => e.JobId);
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
            entity.HasIndex(e => new { e.Puuid, e.Status });

            // Foreign key to Player
            entity.HasOne(e => e.Player)
                .WithMany()
                .HasForeignKey(e => e.Puuid)
                .HasPrincipalKey(p => p.Puuid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Puuid).IsRequired();
            entity.Property(e => e.JobType).IsRequired();
            entity.Property(e => e.Status).IsRequired();
        });

        // ====================
        // Champion Entity Configuration
        // ====================
        modelBuilder.Entity<Champion>(entity =>
        {
            // Explicitly map to champions table
            entity.ToTable("champions");

            // Primary key configuration
            entity.HasKey(e => e.champion_id);

            // Column mappings with constraints
            entity.Property(e => e.champion_key)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.champion_name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.title)
                  .HasMaxLength(100);

            entity.Property(e => e.image_url)
                  .HasMaxLength(255);

            entity.Property(e => e.version)
                  .IsRequired()
                  .HasMaxLength(20);

            // Index for fast lookups by champion_key
            entity.HasIndex(e => e.champion_key);
        });

        // ====================
        // Item Entity Configuration
        // ====================
        modelBuilder.Entity<Item>(entity =>
        {
            // Explicitly map to items table
            entity.ToTable("items");

            // Primary key configuration
            entity.HasKey(e => e.item_id);

            // Column mappings with constraints
            entity.Property(e => e.item_name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.description)
                  .HasColumnType("text");

            entity.Property(e => e.gold_cost)
                  .HasDefaultValue(0);

            entity.Property(e => e.image_url)
                  .HasMaxLength(255);

            entity.Property(e => e.version)
                  .IsRequired()
                  .HasMaxLength(20);

            // Index for fast lookups by item name
            entity.HasIndex(e => e.item_name);
        });

    }
}
