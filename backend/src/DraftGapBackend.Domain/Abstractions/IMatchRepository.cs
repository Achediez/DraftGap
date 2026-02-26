using DraftGapBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Domain.Abstractions;

/// <summary>
/// Repository interface for Match entity operations
/// </summary>
public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(string matchId, CancellationToken cancellationToken = default);
    Task<List<Match>> GetUserMatchesAsync(string puuid, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> GetUserMatchCountAsync(string puuid, CancellationToken cancellationToken = default);
    Task<List<MatchParticipant>> GetUserMatchParticipantsAsync(string puuid, int skip, int take, CancellationToken cancellationToken = default);
}
