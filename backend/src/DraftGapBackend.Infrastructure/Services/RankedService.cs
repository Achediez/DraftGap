using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Ranked;
using DraftGapBackend.Domain.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

public class RankedService : IRankedService
{
    private readonly IUserRepository _userRepository;
    private readonly IRankedRepository _rankedRepository;
    private readonly ILogger<RankedService> _logger;

    public RankedService(
        IUserRepository userRepository,
        IRankedRepository rankedRepository,
        ILogger<RankedService> logger)
    {
        _userRepository = userRepository;
        _rankedRepository = rankedRepository;
        _logger = logger;
    }

    public async Task<RankedStatsDto?> GetUserRankedStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RiotPuuid))
            return null;

        var rankedStats = await _rankedRepository.GetPlayerRankedStatsAsync(user.RiotPuuid, cancellationToken);
        var soloQueue = rankedStats.FirstOrDefault(r => r.QueueType == "RANKED_SOLO_5x5");
        var flexQueue = rankedStats.FirstOrDefault(r => r.QueueType == "RANKED_FLEX_SR");

        return new RankedStatsDto
        {
            SoloQueue = soloQueue != null ? MapToRankedQueueStatsDto(soloQueue) : null,
            FlexQueue = flexQueue != null ? MapToRankedQueueStatsDto(flexQueue) : null
        };
    }

    private RankedQueueStatsDto MapToRankedQueueStatsDto(Domain.Entities.PlayerRankedStat stat)
    {
        return new RankedQueueStatsDto
        {
            QueueType = stat.QueueType,
            Tier = stat.Tier,
            Rank = stat.Rank,
            LeaguePoints = stat.LeaguePoints ?? 0,
            Wins = stat.Wins,
            Losses = stat.Losses,
            TotalGames = stat.TotalGames,
            Winrate = Math.Round(stat.Winrate, 2),
            UpdatedAt = stat.UpdatedAt
        };
    }
}
