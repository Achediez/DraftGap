using DraftGapBackend.Application.Dtos.Common;
using DraftGapBackend.Application.Dtos.Matches;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Interfaces;

/// <summary>
/// Service for match history and details
/// </summary>
public interface IMatchService
{
    Task<PaginatedResult<MatchListItemDto>> GetUserMatchesAsync(
        Guid userId,
        PaginationRequest pagination,
        MatchFilterRequest? filter = null,
        CancellationToken cancellationToken = default);

    Task<MatchDetailDto?> GetMatchDetailAsync(
        string matchId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
