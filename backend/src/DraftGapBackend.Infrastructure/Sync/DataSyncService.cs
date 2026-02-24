using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using DraftGapBackend.Infrastructure.Riot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Sync;

public class DataSyncService : IDataSyncService
{
    private readonly IUserRepository _userRepository;
    private readonly IRiotService _riotService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSyncService> _logger;

    // Maximum match IDs fetched per sync cycle — kept conservative for the dev key limit.
    private const int MatchesPerSync = 10;

    public DataSyncService(
        IUserRepository userRepository,
        IRiotService riotService,
        ApplicationDbContext context,
        ILogger<DataSyncService> logger)
    {
        _userRepository = userRepository;
        _riotService = riotService;
        _context = context;
        _logger = logger;
    }

    public async Task<SyncTriggerResult> TriggerSyncForAllUsersAsync()
    {
        var users = await _userRepository.GetAllActiveUsersAsync();
        var eligibleUsers = users
            .Where(u => !string.IsNullOrEmpty(u.RiotPuuid))
            .ToList();

        var createdJobs = new List<SyncJob>();

        foreach (var user in eligibleUsers)
        {
            // Avoid creating duplicate jobs for users already in the queue.
            var hasActiveJob = await _context.SyncJobs
                .AnyAsync(j => j.Puuid == user.RiotPuuid &&
                               (j.Status == "PENDING" || j.Status == "PROCESSING"));

            if (hasActiveJob)
            {
                _logger.LogInformation(
                    "Skipping sync for PUUID {Puuid}: active job already exists.", user.RiotPuuid);
                continue;
            }

            var job = new SyncJob
            {
                Puuid = user.RiotPuuid!,
                JobType = "FULL_SYNC",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };

            _context.SyncJobs.Add(job);
            createdJobs.Add(job);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Sync triggered: {Count} jobs created for {Total} eligible users.",
            createdJobs.Count, eligibleUsers.Count);

        return new SyncTriggerResult(
            createdJobs.Count,
            $"Created {createdJobs.Count} sync jobs. Processing will start shortly.",
            createdJobs.AsReadOnly()
        );
    }

    public async Task<SyncJob> TriggerSyncForUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User {userId} not found.");

        if (string.IsNullOrEmpty(user.RiotPuuid))
            throw new InvalidOperationException($"User {userId} has no linked Riot account.");

        var job = new SyncJob
        {
            Puuid = user.RiotPuuid,
            JobType = "FULL_SYNC",
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow
        };

        _context.SyncJobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task ProcessSyncJobAsync(SyncJob job, CancellationToken cancellationToken)
    {
        job.Status = "PROCESSING";
        job.StartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        try
        {
            // Resolve the owning user by PUUID to get the region for routing.
            var user = await _userRepository.GetByRiotPuuidAsync(job.Puuid);
            if (user == null)
                throw new InvalidOperationException(
                    $"No user found for PUUID {job.Puuid}.");

            var platform = ResolvePlatform(user.Region ?? "euw1");
            var regional = ResolveRegional(platform);

            await SyncRankedStatsAsync(job.Puuid, platform, cancellationToken);
            await SyncMatchHistoryAsync(job.Puuid, regional, platform, cancellationToken);

            user.LastSync = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            job.Status = "COMPLETED";
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Sync job {JobId} completed for PUUID {Puuid}.", job.JobId, job.Puuid);
        }
        catch (Exception ex)
        {
            job.Status = "FAILED";
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogError(ex,
                "Sync job {JobId} failed for PUUID {Puuid}.", job.JobId, job.Puuid);
        }
        finally
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task<SyncStatusResult> GetSyncStatusAsync()
    {
        var counts = await _context.SyncJobs
            .GroupBy(j => j.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var lastCompleted = await _context.SyncJobs
            .Where(j => j.Status == "COMPLETED")
            .OrderByDescending(j => j.CompletedAt)
            .Select(j => j.CompletedAt)
            .FirstOrDefaultAsync();

        return new SyncStatusResult(
            PendingJobs: counts.FirstOrDefault(c => c.Status == "PENDING")?.Count ?? 0,
            ProcessingJobs: counts.FirstOrDefault(c => c.Status == "PROCESSING")?.Count ?? 0,
            CompletedJobs: counts.FirstOrDefault(c => c.Status == "COMPLETED")?.Count ?? 0,
            FailedJobs: counts.FirstOrDefault(c => c.Status == "FAILED")?.Count ?? 0,
            LastCompletedAt: lastCompleted
        );
    }

    private async Task SyncRankedStatsAsync(
        string puuid, string platform, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rankedStats = await _riotService.GetRankedStatsByPuuidAsync(puuid, platform);
        if (rankedStats == null || rankedStats.Count == 0)
            return;

        foreach (var stat in rankedStats)
        {
            var existing = await _context.PlayerRankedStats
                .FirstOrDefaultAsync(r =>
                    r.Puuid == puuid &&
                    r.QueueType == stat.QueueType);

            if (existing != null)
            {
                existing.Tier = stat.Tier;
                existing.Rank = stat.Rank;
                existing.LeaguePoints = stat.LeaguePoints;
                existing.Wins = stat.Wins;
                existing.Losses = stat.Losses;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.PlayerRankedStats.Add(new PlayerRankedStat
                {
                    Puuid = puuid,
                    QueueType = stat.QueueType,
                    Tier = stat.Tier,
                    Rank = stat.Rank,
                    LeaguePoints = stat.LeaguePoints,
                    Wins = stat.Wins,
                    Losses = stat.Losses,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SyncMatchHistoryAsync(
        string puuid, string regional, string platform, CancellationToken cancellationToken)
    {
        var matchIds = await _riotService.GetMatchIdsByPuuidAsync(puuid, regional, MatchesPerSync);
        if (matchIds == null || matchIds.Count == 0)
            return;

        foreach (var matchId in matchIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // The Match PK IS the Riot match ID string — skip if already persisted.
            var alreadyStored = await _context.Matches
                .AnyAsync(m => m.MatchId == matchId);

            if (alreadyStored)
                continue;

            var matchDto = await _riotService.GetMatchByIdAsync(matchId, regional);
            if (matchDto == null)
                continue;

            var match = new Match
            {
                MatchId = matchDto.Metadata.MatchId,
                GameMode = matchDto.Info.GameMode,
                GameType = matchDto.Info.GameType,
                GameVersion = matchDto.Info.GameVersion,
                GameDuration = matchDto.Info.GameDuration,
                // Riot returns creation time as Unix milliseconds — store as-is in the long column.
                GameCreation = matchDto.Info.GameCreation,
                QueueId = matchDto.Info.QueueId,
                PlatformId = matchDto.Info.PlatformId,
                FetchedAt = DateTime.UtcNow
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            foreach (var p in matchDto.Info.Participants)
            {
                _context.MatchParticipants.Add(new MatchParticipant
                {
                    MatchId = match.MatchId,
                    Puuid = p.Puuid,
                    RiotIdGameName = p.RiotIdGameName,
                    ChampionId = p.ChampionId,
                    ChampionName = p.ChampionName,
                    ChampLevel = p.ChampLevel,
                    TeamId = p.TeamId,
                    TeamPosition = p.TeamPosition,
                    Kills = p.Kills,
                    Deaths = p.Deaths,
                    Assists = p.Assists,
                    Win = p.Win,
                    GoldEarned = p.GoldEarned,
                    TotalDamageDealt = p.TotalDamageDealt,
                    TotalDamageDealtToChampions = p.TotalDamageDealtToChampions,
                    TotalDamageTaken = p.TotalDamageTaken,
                    // Riot separates minion and neutral CS — sum them into the single Cs column.
                    Cs = p.TotalMinionsKilled + p.NeutralMinionsKilled,
                    VisionScore = p.VisionScore,
                    DoubleKills = p.DoubleKills,
                    TripleKills = p.TripleKills,
                    QuadraKills = p.QuadraKills,
                    PentaKills = p.PentaKills,
                    FirstBlood = p.FirstBloodKill,
                    Item0 = p.Item0,
                    Item1 = p.Item1,
                    Item2 = p.Item2,
                    Item3 = p.Item3,
                    Item4 = p.Item4,
                    Item5 = p.Item5,
                    Item6 = p.Item6,
                    Summoner1Id = p.Summoner1Id,
                    Summoner2Id = p.Summoner2Id,
                    PerkPrimaryStyle = p.Perks?.Styles?.FirstOrDefault(s => s.Description == "primaryStyle")?.Style,
                    PerkSubStyle = p.Perks?.Styles?.FirstOrDefault(s => s.Description == "subStyle")?.Style
                });
            }

            await _context.SaveChangesAsync();

            // 500ms delay between match fetches keeps concurrent requests safely within
            // the dev key rate limit of 20 requests per second.
            await Task.Delay(500, cancellationToken);
        }
    }

    private static string ResolvePlatform(string region) => region.ToLower() switch
    {
        "euw1" or "euw" => "euw1",
        "eune" or "eun1" => "eun1",
        "na1" or "na" => "na1",
        "kr" => "kr",
        "br1" or "br" => "br1",
        "la1" or "lan" => "la1",
        "la2" or "las" => "la2",
        "oc1" or "oce" => "oc1",
        "tr1" or "tr" => "tr1",
        "ru" => "ru",
        _ => "euw1"
    };

    private static string ResolveRegional(string platform) => platform switch
    {
        "euw1" or "eun1" or "tr1" or "ru" => "europe",
        "na1" or "br1" or "la1" or "la2" => "americas",
        "kr" or "jp1" => "asia",
        "oc1" => "sea",
        _ => "europe"
    };
}
