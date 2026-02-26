using System;

namespace DraftGapBackend.Application.Dtos.Sync;

/// <summary>
/// Request to trigger manual sync for the authenticated user
/// </summary>
public class TriggerSyncRequest
{
    public bool ForceRefresh { get; set; } = false;
}

/// <summary>
/// Sync job status response
/// </summary>
public class SyncJobDto
{
    public int JobId { get; set; }
    public string Puuid { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// User's sync history
/// </summary>
public class UserSyncHistoryDto
{
    public DateTime? LastSync { get; set; }
    public int TotalSyncs { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int FailedSyncs { get; set; }
    public SyncJobDto? LatestJob { get; set; }
}
