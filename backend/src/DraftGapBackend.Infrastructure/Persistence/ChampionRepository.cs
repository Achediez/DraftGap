using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Persistence;

public class ChampionRepository : IChampionRepository
{
    private readonly ApplicationDbContext _context;

    public ChampionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Champion>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Champions
            .OrderBy(c => c.champion_name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Champion?> GetByIdAsync(int championId, CancellationToken cancellationToken = default)
    {
        return await _context.Champions
            .FirstOrDefaultAsync(c => c.champion_id == championId, cancellationToken);
    }
}
