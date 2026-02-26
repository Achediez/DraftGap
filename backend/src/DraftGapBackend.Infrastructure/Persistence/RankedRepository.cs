using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Persistence;

public class RankedRepository : IRankedRepository
{
    private readonly ApplicationDbContext _context;

    public RankedRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlayerRankedStat>> GetPlayerRankedStatsAsync(string puuid, CancellationToken cancellationToken = default)
    {
        return await _context.PlayerRankedStats
            .Where(r => r.Puuid == puuid)
            .OrderBy(r => r.QueueType)
            .ToListAsync(cancellationToken);
    }
}
