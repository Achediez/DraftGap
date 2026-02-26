using DraftGapBackend.Application.Dtos.Sync;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Interfaces;

/// <summary>
/// Service for user-initiated sync operations
/// </summary>
public interface IUserSyncService
{
    Task<SyncJobDto> TriggerUserSyncAsync(Guid userId, TriggerSyncRequest request, CancellationToken cancellationToken = default);
    Task<UserSyncHistoryDto> GetUserSyncHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
}
