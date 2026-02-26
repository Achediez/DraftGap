using DraftGapBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Domain.Abstractions;

/// <summary>
/// Repository interface for Champion entity operations
/// </summary>
public interface IChampionRepository
{
    Task<List<Champion>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Champion?> GetByIdAsync(int championId, CancellationToken cancellationToken = default);
}
