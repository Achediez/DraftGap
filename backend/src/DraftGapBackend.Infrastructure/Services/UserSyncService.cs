using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Sync;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

public class UserSyncService : IUserSyncService
{
    private readonly IUserRepository _userRepository;
    private readonly IDataSyncService _dataSyncService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserSyncService> _logger;

    public UserSyncService(
        IUserRepository userRepository,
        IDataSyncService dataSyncService,
        ApplicationDbContext context,
        ILogger<UserSyncService> logger)
    {
        _userRepository = userRepository;
        _dataSyncService = dataSyncService;
        _context = context;
        _logger = logger;
    }

    public async Task<SyncJobDto> TriggerUserSyncAsync(Guid userId, TriggerSyncRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RiotPuuid))
            throw new InvalidOperationException("User not found or has no linked Riot account");

        var job = await _dataSyncService.TriggerSyncForUserAsync(userId);

        return new SyncJobDto
        {
            JobId = (int)job.JobId,
            Puuid = job.Puuid,
            JobType = job.JobType,
            Status = job.Status,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            ErrorMessage = job.ErrorMessage
        };
    }

    public async Task<UserSyncHistoryDto> GetUserSyncHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RiotPuuid))
            throw new InvalidOperationException("User not found or has no linked Riot account");

        var puuid = user.RiotPuuid;

        var allJobs = await _context.SyncJobs
            .Where(j => j.Puuid == puuid)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);

        var latestJob = allJobs.FirstOrDefault();

        return new UserSyncHistoryDto
        {
            LastSync = user.LastSync,
            TotalSyncs = allJobs.Count,
            SuccessfulSyncs = allJobs.Count(j => j.Status == "COMPLETED"),
            FailedSyncs = allJobs.Count(j => j.Status == "FAILED"),
            LatestJob = latestJob != null ? new SyncJobDto
            {
                JobId = (int)latestJob.JobId,
                Puuid = latestJob.Puuid,
                JobType = latestJob.JobType,
                Status = latestJob.Status,
                CreatedAt = latestJob.CreatedAt,
                StartedAt = latestJob.StartedAt,
                CompletedAt = latestJob.CompletedAt,
                ErrorMessage = latestJob.ErrorMessage
            } : null
        };
    }
}
