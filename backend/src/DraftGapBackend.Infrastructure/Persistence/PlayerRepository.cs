using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Persistence;

/// <summary>
/// Repositorio para acceso a datos de summoners/players.
/// Responsabilidades:
/// - Obtener datos de player por PUUID
/// - Include de RankedStats relacionados
/// Datos actualizados por el sistema de sync desde Riot API.
/// </summary>
public class PlayerRepository : IPlayerRepository
{
    private readonly ApplicationDbContext _context;

    public PlayerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Player?> GetByPuuidAsync(string puuid, CancellationToken cancellationToken = default)
    {
        return await _context.Players
            .Include(p => p.RankedStats)
            .FirstOrDefaultAsync(p => p.Puuid == puuid, cancellationToken);
    }
}
