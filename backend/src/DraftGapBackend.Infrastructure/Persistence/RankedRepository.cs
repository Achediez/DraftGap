using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Persistence;

/// <summary>
/// Repositorio para acceso a estadísticas de ranked.
/// Responsabilidades:
/// - Obtener stats de ranked por PUUID
/// - Soportar múltiples queue types (Solo/Duo, Flex)
/// Datos actualizados por el sistema de sync desde Riot API.
/// </summary>
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
