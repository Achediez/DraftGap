using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Persistence;

public class MatchRepository : IMatchRepository
{
    private readonly ApplicationDbContext _context;

    public MatchRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Match?> GetByIdAsync(string matchId, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.MatchId == matchId, cancellationToken);
    }

    public async Task<List<Match>> GetUserMatchesAsync(string puuid, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Where(m => m.Participants.Any(p => p.Puuid == puuid))
            .OrderByDescending(m => m.GameCreation)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUserMatchCountAsync(string puuid, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Where(m => m.Participants.Any(p => p.Puuid == puuid))
            .CountAsync(cancellationToken);
    }

    public async Task<List<MatchParticipant>> GetUserMatchParticipantsAsync(string puuid, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.MatchParticipants
            .Where(p => p.Puuid == puuid)
            .Include(p => p.Match)
            .OrderByDescending(p => p.Match.GameCreation)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
