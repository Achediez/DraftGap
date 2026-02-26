using DraftGapBackend.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Domain.Abstractions;

/// <summary>
/// Repository interface for Player entity operations
/// </summary>
public interface IPlayerRepository
{
    Task<Player?> GetByPuuidAsync(string puuid, CancellationToken cancellationToken = default);
}
