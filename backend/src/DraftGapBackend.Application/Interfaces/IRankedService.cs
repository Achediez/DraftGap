using DraftGapBackend.Application.Ranked;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Interfaces;

/// <summary>
/// Service for ranked statistics
/// </summary>
public interface IRankedService
{
    Task<RankedStatsDto?> GetUserRankedStatsAsync(Guid userId, CancellationToken cancellationToken = default);
}
