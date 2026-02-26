using DraftGapBackend.Application.Dashboard;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IRankedRepository _rankedRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IUserRepository userRepository,
        IRankedRepository rankedRepository,
        IMatchRepository matchRepository,
        ILogger<DashboardService> logger)
    {
        _userRepository = userRepository;
        _rankedRepository = rankedRepository;
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RiotPuuid))
            throw new InvalidOperationException("User not found or has no linked Riot account");

        var puuid = user.RiotPuuid;

        // Get ranked stats
        var rankedStats = await _rankedRepository.GetPlayerRankedStatsAsync(puuid, cancellationToken);
        var soloQueue = rankedStats.FirstOrDefault(r => r.QueueType == "RANKED_SOLO_5x5");
        var flexQueue = rankedStats.FirstOrDefault(r => r.QueueType == "RANKED_FLEX_SR");

        var rankedOverview = new RankedOverviewDto
        {
            SoloQueue = soloQueue != null ? MapToRankedQueueDto(soloQueue) : null,
            FlexQueue = flexQueue != null ? MapToRankedQueueDto(flexQueue) : null
        };

        // Get recent matches (last 10)
        var recentParticipants = await _matchRepository.GetUserMatchParticipantsAsync(puuid, 0, 10, cancellationToken);
        var recentMatches = recentParticipants.Select(p => new RecentMatchDto
        {
            MatchId = p.MatchId,
            GameCreation = p.Match?.GameCreation ?? 0,
            GameDuration = p.Match?.GameDuration ?? 0,
            ChampionName = p.ChampionName,
            Win = p.Win,
            Kills = p.Kills,
            Deaths = p.Deaths,
            Assists = p.Assists,
            Kda = p.Deaths > 0 ? Math.Round((double)(p.Kills + p.Assists) / p.Deaths, 2) : p.Kills + p.Assists,
            TeamPosition = p.TeamPosition
        }).ToList();

        // Get performance stats (last 20 matches)
        var allParticipants = await _matchRepository.GetUserMatchParticipantsAsync(puuid, 0, 20, cancellationToken);
        var performanceStats = CalculatePerformanceStats(allParticipants);

        // Get top champions (based on last 50 matches)
        var championParticipants = await _matchRepository.GetUserMatchParticipantsAsync(puuid, 0, 50, cancellationToken);
        var topChampions = championParticipants
            .GroupBy(p => new { p.ChampionId, p.ChampionName })
            .Select(g => new TopChampionDto
            {
                ChampionId = g.Key.ChampionId,
                ChampionName = g.Key.ChampionName,
                GamesPlayed = g.Count(),
                Wins = g.Count(p => p.Win),
                Winrate = g.Count() > 0 ? Math.Round((double)g.Count(p => p.Win) / g.Count() * 100, 2) : 0,
                AvgKda = g.Average(p => p.Deaths > 0 ? (double)(p.Kills + p.Assists) / p.Deaths : p.Kills + p.Assists)
            })
            .OrderByDescending(c => c.GamesPlayed)
            .Take(5)
            .ToList();

        return new DashboardSummaryDto
        {
            RankedOverview = rankedOverview,
            RecentMatches = recentMatches,
            PerformanceStats = performanceStats,
            TopChampions = topChampions
        };
    }

    private RankedQueueDto MapToRankedQueueDto(Domain.Entities.PlayerRankedStat stat)
    {
        return new RankedQueueDto
        {
            QueueType = stat.QueueType,
            Tier = stat.Tier,
            Rank = stat.Rank,
            LeaguePoints = stat.LeaguePoints ?? 0,
            Wins = stat.Wins,
            Losses = stat.Losses,
            TotalGames = stat.TotalGames,
            Winrate = Math.Round(stat.Winrate, 2)
        };
    }

    private PerformanceStatsDto? CalculatePerformanceStats(System.Collections.Generic.List<Domain.Entities.MatchParticipant> participants)
    {
        if (!participants.Any())
            return null;

        var totalMatches = participants.Count;
        var wins = participants.Count(p => p.Win);

        return new PerformanceStatsDto
        {
            TotalMatches = totalMatches,
            Wins = wins,
            Losses = totalMatches - wins,
            Winrate = Math.Round((double)wins / totalMatches * 100, 2),
            AvgKills = Math.Round(participants.Average(p => p.Kills), 2),
            AvgDeaths = Math.Round(participants.Average(p => p.Deaths), 2),
            AvgAssists = Math.Round(participants.Average(p => p.Assists), 2),
            AvgKda = Math.Round(participants.Average(p => p.Deaths > 0 ? (double)(p.Kills + p.Assists) / p.Deaths : p.Kills + p.Assists), 2)
        };
    }
}
