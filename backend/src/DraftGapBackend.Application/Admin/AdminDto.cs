using System;
using System.Collections.Generic;

namespace DraftGapBackend.Application.Admin;

/// <summary>
/// Admin user list item with sync state
/// </summary>
public class AdminUserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? RiotId { get; set; }
    public string? Region { get; set; }
    public DateTime? LastSync { get; set; }
    public bool HasPuuid { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// System statistics for admin dashboard
/// </summary>
public class SystemStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalMatches { get; set; }
    public int MatchesToday { get; set; }
    public int PendingSyncJobs { get; set; }
    public int FailedSyncJobs { get; set; }
    public DateTime? LastSyncTime { get; set; }
}

/// <summary>
/// Sync status for admin monitoring
/// </summary>
public class AdminSyncStatusDto
{
    public int PendingJobs { get; set; }
    public int ProcessingJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int FailedJobs { get; set; }
    public DateTime? LastCompletedAt { get; set; }
}

/// <summary>
/// Request to trigger sync for specific users or all users
/// </summary>
public class AdminSyncRequest
{
    public string SyncType { get; set; } = "FULL_SYNC";
    public bool ForceRefresh { get; set; } = false;
    public List<Guid>? UserIds { get; set; }
}

/// <summary>
/// Response when sync jobs are created
/// </summary>
public class SyncTriggerResponseDto
{
    public int JobsCreated { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<SyncJobInfoDto> Jobs { get; set; } = [];
}

/// <summary>
/// Sync job information
/// </summary>
public class SyncJobInfoDto
{
    public int JobId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
