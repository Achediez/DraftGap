using DraftGapBackend.Infrastructure.Riot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Riot;

/// <summary>
/// Client service for interacting with Riot Games HTTP APIs.
/// Encapsulates HTTP calls and mapping to DTOs used by the application.
/// Uses configuration keys under <c>RiotApi</c> for API URLs and API key.
/// </summary>
public class RiotService : IRiotService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RiotService> _logger;
    private readonly string _apiKey;

    /// <summary>
    /// Initializes a new instance of <see cref="RiotService"/>.
    /// </summary>
    /// <param name="httpClient">HttpClient provided by IHttpClientFactory.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Logger instance.</param>
    public RiotService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<RiotService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["RiotApi:ApiKey"] ?? throw new InvalidOperationException("Riot API key not configured");
    }

    /// <summary>
    /// Retrieves a Riot account by Riot ID (gameName and tagLine) using the regional account API.
    /// </summary>
    /// <param name="gameName">Game name portion of the Riot ID.</param>
    /// <param name="tagLine">Tag line portion of the Riot ID.</param>
    /// <param name="region">Regional key used to resolve the regional API base URL.</param>
    /// <returns>RiotAccountDto when found; otherwise null.</returns>
    public async Task<RiotAccountDto?> GetAccountByRiotIdAsync(string gameName, string tagLine, string region = "europe")
    {
        var regionalUrl = _configuration[$"RiotApi:RegionalUrls:{region}"]
            ?? "https://europe.api.riotgames.com";

        var requestUrl = $"{regionalUrl}/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}";

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Riot account: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<RiotAccountDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Riot account for {GameName}#{TagLine}", gameName, tagLine);
            return null;
        }
    }

    /// <summary>
    /// Retrieves summoner information by PUUID from the platform API.
    /// </summary>
    /// <param name="puuid">Player unique identifier (PUUID).</param>
    /// <param name="platform">Platform key (e.g. "euw1").</param>
    /// <returns>SummonerDto when found; otherwise null.</returns>
    public async Task<SummonerDto?> GetSummonerByPuuidAsync(string puuid, string platform = "euw1")
    {
        var platformUrl = _configuration[$"RiotApi:PlatformUrls:{platform}"]
            ?? "https://euw1.api.riotgames.com";

        var requestUrl = $"{platformUrl}/lol/summoner/v4/summoners/by-puuid/{puuid}";

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get summoner: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SummonerDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summoner for PUUID {Puuid}", puuid);
            return null;
        }
    }

    /// <summary>
    /// Retrieves ranked stats for a player identified by PUUID. Internally resolves summoner id then calls league API.
    /// </summary>
    /// <param name="puuid">Player PUUID.</param>
    /// <param name="platform">Platform key.</param>
    /// <returns>List of ranked entries; empty list if none or on error.</returns>
    public async Task<List<RankedStatsDto>> GetRankedStatsByPuuidAsync(string puuid, string platform = "euw1")
    {
        var platformUrl = _configuration[$"RiotApi:PlatformUrls:{platform}"]
            ?? "https://euw1.api.riotgames.com";

        // First get summoner ID from PUUID
        var summoner = await GetSummonerByPuuidAsync(puuid, platform);
        if (summoner == null)
        {
            return new List<RankedStatsDto>();
        }

        var requestUrl = $"{platformUrl}/lol/league/v4/entries/by-summoner/{summoner.Id}";

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get ranked stats: {StatusCode}", response.StatusCode);
                return new List<RankedStatsDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<RankedStatsDto>>()
                ?? new List<RankedStatsDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ranked stats for PUUID {Puuid}", puuid);
            return new List<RankedStatsDto>();
        }
    }

    /// <summary>
    /// Retrieves recent match IDs for a player by PUUID via the regional match API.
    /// </summary>
    /// <param name="puuid">Player PUUID.</param>
    /// <param name="region">Regional key used for the regional API base URL.</param>
    /// <param name="count">Maximum number of match ids to retrieve.</param>
    /// <returns>List of match id strings; empty list on error.</returns>
    public async Task<List<string>> GetMatchIdsByPuuidAsync(string puuid, string region = "europe", int count = 20)
    {
        var regionalUrl = _configuration[$"RiotApi:RegionalUrls:{region}"]
            ?? "https://europe.api.riotgames.com";

        var requestUrl = $"{regionalUrl}/lol/match/v5/matches/by-puuid/{puuid}/ids?count={count}";

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get match IDs: {StatusCode}", response.StatusCode);
                return new List<string>();
            }

            return await response.Content.ReadFromJsonAsync<List<string>>()
                ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting match IDs for PUUID {Puuid}", puuid);
            return new List<string>();
        }
    }

    /// <summary>
    /// Retrieves a match by its id using the regional match API.
    /// </summary>
    /// <param name="matchId">Match identifier.</param>
    /// <param name="region">Regional key.</param>
    /// <returns>MatchDto when found; otherwise null.</returns>
    public async Task<MatchDto?> GetMatchByIdAsync(string matchId, string region = "europe")
    {
        var regionalUrl = _configuration[$"RiotApi:RegionalUrls:{region}"]
            ?? "https://europe.api.riotgames.com";

        var requestUrl = $"{regionalUrl}/lol/match/v5/matches/{matchId}";

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get match {MatchId}: {StatusCode}", matchId, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MatchDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting match {MatchId}", matchId);
            return null;
        }
    }

    /// <summary>
    /// Returns a simple rate limit status. Currently an in-memory stub for diagnostics.
    /// Replace with Redis-based tracking for production scenarios.
    /// </summary>
    /// <returns>RateLimitStatus with current limits and remaining counts.</returns>
    public Task<RateLimitStatus> GetRateLimitStatusAsync()
    {
        // Simple in-memory rate limit tracking
        // For production, use Redis
        return Task.FromResult(new RateLimitStatus
        {
            RequestsPerSecond = 20,
            RequestsPer2Minutes = 100,
            RemainingPerSecond = 20,
            RemainingPer2Minutes = 100,
            ResetTimePerSecond = DateTime.UtcNow.AddSeconds(1),
            ResetTimePer2Minutes = DateTime.UtcNow.AddMinutes(2)
        });
    }
}
