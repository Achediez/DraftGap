using System.Collections.Generic;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Riot;

public interface IRiotService
{
    Task<RiotAccountDto?> GetAccountByRiotIdAsync(string gameName, string tagLine, string region = "europe");
    Task<SummonerDto?> GetSummonerByPuuidAsync(string puuid, string platform = "euw1");
    Task<List<RankedStatsDto>> GetRankedStatsByPuuidAsync(string puuid, string platform = "euw1");
    Task<List<string>> GetMatchIdsByPuuidAsync(string puuid, string region = "europe", int count = 20);
    Task<MatchDto?> GetMatchByIdAsync(string matchId, string region = "europe");
    Task<RateLimitStatus> GetRateLimitStatusAsync();
}
