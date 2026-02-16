using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Riot;

/// <summary>
/// Implementation of Data Dragon synchronization service.
/// Fetches static game data from Riot's public CDN (no API key required).
/// Data is fetched once on startup and stored in local database for performance.
/// </summary>
public class DataDragonService : IDataDragonService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataDragonService> _logger;

    /// <summary>
    /// Base URL for Data Dragon CDN.
    /// All static data files are served from this CDN with no authentication required.
    /// </summary>
    private const string DdragonBase = "https://ddragon.leagueoflegends.com/cdn";

    /// <summary>
    /// API endpoint that returns list of all available patch versions.
    /// First element is always the latest version.
    /// Example response: ["16.3.1", "16.2.1", "16.1.1", ...]
    /// </summary>
    private const string LatestVersionUrl = "https://ddragon.leagueoflegends.com/api/versions.json";

    public DataDragonService(
        HttpClient httpClient,
        ApplicationDbContext context,
        ILogger<DataDragonService> logger)
    {
        _httpClient = httpClient;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Synchronizes champion data from Data Dragon to local database.
    /// Process:
    /// 1. Fetch latest patch version from versions API
    /// 2. Download champion.json for that version (Spanish locale)
    /// 3. Check if champions already exist in database
    /// 4. Parse JSON and insert champion records
    /// </summary>
    public async Task SyncChampionsAsync(CancellationToken ct = default)
    {
        try
        {
            // Configure JSON deserialization to handle property name casing differences
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Step 1: Fetch latest game version
            _logger.LogInformation("Fetching latest patch version from Data Dragon API");
            var versionsJson = await _httpClient.GetStringAsync(LatestVersionUrl, ct);
            var versions = JsonSerializer.Deserialize<List<string>>(versionsJson, jsonOptions);
            var latestVersion = versions?.FirstOrDefault() ?? "14.3.1";

            _logger.LogInformation("Latest patch version detected: {Version}", latestVersion);

            // Step 2: Construct URL for Spanish champion data
            var championsUrl = $"{DdragonBase}/{latestVersion}/data/es_ES/champion.json";
            _logger.LogInformation("Fetching champion data from: {Url}", championsUrl);

            var championsJson = await _httpClient.GetStringAsync(championsUrl, ct);
            var response = JsonSerializer.Deserialize<DdragonChampionsResponse>(championsJson, jsonOptions);

            // Step 3: Validate parsed data
            if (response?.Data == null || response.Data.Count == 0)
            {
                _logger.LogWarning("No champion data received or parsed from Data Dragon");
                _logger.LogDebug("Raw JSON preview: {Preview}",
                    championsJson.Substring(0, Math.Min(500, championsJson.Length)));
                return;
            }

            _logger.LogInformation("Successfully parsed {Count} champions from Data Dragon", response.Data.Count);

            // Step 4: Check if data already exists to avoid duplicate inserts
            var existingCount = await _context.Champions.CountAsync(ct);
            if (existingCount > 0)
            {
                _logger.LogInformation("Champions already exist in database ({Count} records), skipping sync",
                    existingCount);
                return;
            }

            // Step 5: Transform Data Dragon DTOs to domain entities
            var champions = response.Data.Values.Select(c => new Champion
            {
                champion_id = int.Parse(c.Key),
                champion_key = c.Id,
                champion_name = c.Name,
                title = c.Title,
                image_url = $"{DdragonBase}/{latestVersion}/img/champion/{c.Image.Full}",
                version = latestVersion
            }).ToList();

            // Step 6: Persist to database
            await _context.Champions.AddRangeAsync(champions, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully synced {Count} champions to database for patch {Version}",
                champions.Count, latestVersion);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching Data Dragon content");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Data Dragon JSON response");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during champion synchronization");
            throw;
        }
    }
}

// ====================================
// Data Dragon JSON Response DTOs
// ====================================

/// <summary>
/// Root response structure from Data Dragon champion.json endpoint.
/// Contains metadata about the data set and a dictionary of all champions.
/// </summary>
public class DdragonChampionsResponse
{
    /// <summary>
    /// Type of data returned. Always "champion" for champion.json.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Patch version of this data set.
    /// Example: "16.3.1"
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Dictionary of champions keyed by champion key.
    /// Key examples: "Aatrox", "Ahri", "MonkeyKing"
    /// </summary>
    [JsonPropertyName("data")]
    public Dictionary<string, DdragonChampion> Data { get; set; } = new();
}

/// <summary>
/// Individual champion data structure from Data Dragon.
/// Contains all metadata needed to display champion information.
/// </summary>
public class DdragonChampion
{
    /// <summary>
    /// Champion's key name used in APIs and URLs.
    /// Example: "Aatrox", "Ahri", "MonkeyKing"
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Riot's numeric champion ID.
    /// Example: "266" for Aatrox, "103" for Ahri
    /// Stored as string in JSON but parsed to int in database.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Localized champion display name.
    /// Language depends on locale in URL (es_ES, en_US, etc.)
    /// Example (es_ES): "Aatrox", "Ahri", "Wukong"
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Localized champion title or tagline.
    /// Example (es_ES): "la Espada de los Oscuros", "la Mujer Zorro de Nueve Colas"
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Image metadata object containing filenames for champion icons.
    /// </summary>
    [JsonPropertyName("image")]
    public DdragonImage Image { get; set; } = new();
}

/// <summary>
/// Image metadata structure from Data Dragon.
/// Contains filename for champion portrait image.
/// </summary>
public class DdragonImage
{
    /// <summary>
    /// Filename of the champion's square portrait image.
    /// Example: "Aatrox.png", "Ahri.png"
    /// Must be combined with CDN base URL to create full image path.
    /// </summary>
    [JsonPropertyName("full")]
    public string Full { get; set; } = string.Empty;
}
