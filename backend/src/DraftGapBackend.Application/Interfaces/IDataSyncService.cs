using DraftGapBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Interfaces;

// Defines the contract for orchestrating Riot API data synchronization per user.
public interface IDataSyncService
{
    // Enqueues sync jobs for all active users that have a valid PUUID.
    Task<SyncTriggerResult> TriggerSyncForAllUsersAsync();

    // Enqueues a sync job for a single user identified by their database ID.
    Task<SyncJob> TriggerSyncForUserAsync(Guid userId);

    // Executes a single pending SyncJob: fetches ranked stats and match history from Riot API.
    Task ProcessSyncJobAsync(SyncJob job, CancellationToken cancellationToken);

    // Returns the current aggregate status of all sync jobs.
    Task<SyncStatusResult> GetSyncStatusAsync();
}

// Result returned when triggering a bulk sync operation.
public record SyncTriggerResult(int JobsCreated, string Message, IReadOnlyList<SyncJob> Jobs);

// Aggregate status snapshot for the admin panel status endpoint.
public record SyncStatusResult(
    int PendingJobs,
    int ProcessingJobs,
    int CompletedJobs,
    int FailedJobs,
    DateTime? LastCompletedAt
);
