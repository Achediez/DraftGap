using DraftGapBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Domain.Abstractions;

/// <summary>
/// Repository interface for PlayerRankedStat entity operations
/// </summary>
public interface IRankedRepository
{
    Task<List<PlayerRankedStat>> GetPlayerRankedStatsAsync(string puuid, CancellationToken cancellationToken = default);
}
