using DraftGapBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DraftGapBackend.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for DraftGap application.
/// Maps C# domain entities to MySQL tables.
/// Entity configurations are externalized to separate IEntityTypeConfiguration files
/// located in the Configurations folder for better maintainability.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ====================================
    // DbSets - Database Tables
    // ====================================

    /// <summary>
    /// User authentication and profile data.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Riot player data linked to user accounts.
    /// </summary>
    public DbSet<Player> Players { get; set; }

    /// <summary>
    /// Match metadata from Riot API.
    /// </summary>
    public DbSet<Match> Matches { get; set; }

    /// <summary>
    /// Individual player performance data within matches.
    /// </summary>
    public DbSet<MatchParticipant> MatchParticipants { get; set; }

    /// <summary>
    /// Player ranked queue statistics (Solo/Duo, Flex, etc.).
    /// </summary>
    public DbSet<PlayerRankedStat> PlayerRankedStats { get; set; }

    /// <summary>
    /// Background job tracking for Riot API data synchronization.
    /// </summary>
    public DbSet<SyncJob> SyncJobs { get; set; }

    /// <summary>
    /// Static champion data from Data Dragon (Aatrox, Ahri, etc.).
    /// </summary>
    public DbSet<Champion> Champions { get; set; }

    /// <summary>
    /// Static item data from Data Dragon (Trinity Force, etc.).
    /// </summary>
    public DbSet<Item> Items { get; set; }

    /// <summary>
    /// Static summoner spell data from Data Dragon (Flash, Ignite, etc.).
    /// </summary>
    public DbSet<SummonerSpell> SummonerSpells { get; set; }

    // ====================================
    // Model Configuration
    // ====================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration classes from Configurations folder
        // This automatically discovers and applies ChampionConfiguration, ItemConfiguration, etc.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
