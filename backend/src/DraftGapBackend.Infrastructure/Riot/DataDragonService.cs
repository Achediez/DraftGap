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

    /// <summary>
    /// Synchronizes item data from Data Dragon to local database.
    /// Process:
    /// 1. Uses latest patch version from versions API
    /// 2. Downloads item.json for that version (Spanish locale)
    /// 3. Checks if items already exist in database
    /// 4. Parses JSON and inserts item records
    /// Matches database schema: item_id, item_name, description, gold_cost, image_url, version
    /// </summary>
    public async Task SyncItemsAsync(CancellationToken ct = default)
    {
        try
        {
            // Configure JSON deserialization
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Step 1: Fetch latest game version
            var versionsJson = await _httpClient.GetStringAsync(LatestVersionUrl, ct);
            var versions = JsonSerializer.Deserialize<List<string>>(versionsJson, jsonOptions);
            var latestVersion = versions?.FirstOrDefault() ?? "14.3.1";

            _logger.LogInformation("Fetching item data for patch {Version} (es_ES)", latestVersion);

            // Step 2: Construct URL for Spanish item data
            var itemsUrl = $"{DdragonBase}/{latestVersion}/data/es_ES/item.json";
            _logger.LogInformation("Fetching items from: {Url}", itemsUrl);

            var itemsJson = await _httpClient.GetStringAsync(itemsUrl, ct);
            var response = JsonSerializer.Deserialize<DdragonItemsResponse>(itemsJson, jsonOptions);

            // Step 3: Validate parsed data
            if (response?.Data == null || response.Data.Count == 0)
            {
                _logger.LogWarning("No item data received or parsed from Data Dragon");
                return;
            }

            _logger.LogInformation("Successfully parsed {Count} items from Data Dragon", response.Data.Count);

            // Step 4: Check if data already exists to avoid duplicate inserts
            var existingCount = await _context.Items.CountAsync(ct);
            if (existingCount > 0)
            {
                _logger.LogInformation("Items already exist in database ({Count} records), skipping sync", existingCount);
                return;
            }

            // Step 5: Transform Data Dragon DTOs to domain entities
            // Filter: only numeric item IDs (excludes special items like boots upgrades)
            var items = response.Data
                .Where(kvp => int.TryParse(kvp.Key, out _))
                .Select(kvp => new Item
                {
                    item_id = int.Parse(kvp.Key),
                    item_name = kvp.Value.Name,
                    description = kvp.Value.Description,
                    gold_cost = kvp.Value.Gold.Total,
                    image_url = $"{DdragonBase}/{latestVersion}/img/item/{kvp.Value.Image.Full}",
                    version = latestVersion
                }).ToList();

            // Step 6: Persist to database
            await _context.Items.AddRangeAsync(items, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully synced {Count} items to database for patch {Version}",
                items.Count, latestVersion);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching item data from Data Dragon");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Data Dragon item JSON response");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during item synchronization");
            throw;
        }
    }

    /// <summary>
    /// Synchronizes summoner spell data from Data Dragon to local database.
    /// Process:
    /// 1. Uses latest patch version
    /// 2. Downloads summoner.json (Spanish locale)
    /// 3. Checks if spells already exist
    /// 4. Parses and inserts spell records
    /// Filters out deprecated/unused spells (e.g., old Clairvoyance).
    /// </summary>
    public async Task SyncSummonerSpellsAsync(CancellationToken ct = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Step 1: Fetch latest game version
            var versionsJson = await _httpClient.GetStringAsync(LatestVersionUrl, ct);
            var versions = JsonSerializer.Deserialize<List<string>>(versionsJson, jsonOptions);
            var latestVersion = versions?.FirstOrDefault() ?? "14.3.1";

            _logger.LogInformation("Fetching summoner spell data for patch {Version} (es_ES)", latestVersion);

            // Step 2: Construct URL for Spanish summoner spell data
            var spellsUrl = $"{DdragonBase}/{latestVersion}/data/es_ES/summoner.json";
            _logger.LogInformation("Fetching summoner spells from: {Url}", spellsUrl);

            var spellsJson = await _httpClient.GetStringAsync(spellsUrl, ct);
            var response = JsonSerializer.Deserialize<DdragonSummonerSpellsResponse>(spellsJson, jsonOptions);

            // Step 3: Validate parsed data
            if (response?.Data == null || response.Data.Count == 0)
            {
                _logger.LogWarning("No summoner spell data received or parsed from Data Dragon");
                return;
            }

            _logger.LogInformation("Successfully parsed {Count} summoner spells from Data Dragon", response.Data.Count);

            // Step 4: Check if data already exists
            var existingCount = await _context.SummonerSpells.CountAsync(ct);
            if (existingCount > 0)
            {
                _logger.LogInformation("Summoner spells already exist in database ({Count} records), skipping sync",
                    existingCount);
                return;
            }

            // Step 5: Transform DTOs to domain entities
            // Filter: only spells with valid numeric IDs (excludes deprecated spells)
            var spells = response.Data.Values
                .Where(s => int.TryParse(s.Key, out _) && !string.IsNullOrEmpty(s.Name))
                .Select(s => new SummonerSpell
                {
                    spell_id = int.Parse(s.Key),
                    spell_key = s.Id,
                    spell_name = s.Name,
                    description = s.Description,
                    cooldown = s.Cooldown.Any() ? (int)s.Cooldown.First() : 0,  // Convert double → int, default 0 if empty
                    image_url = $"{DdragonBase}/{latestVersion}/img/spell/{s.Image.Full}",
                    version = latestVersion
                }).ToList();

            // Step 6: Persist to database
            await _context.SummonerSpells.AddRangeAsync(spells, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully synced {Count} summoner spells to database for patch {Version}",
                spells.Count, latestVersion);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching summoner spell data from Data Dragon");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Data Dragon summoner spell JSON response");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during summoner spell synchronization");
            throw;
        }
    }


}

// ========================================
// Data Dragon Champions JSON Response DTOs
// ========================================

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

// ====================================
// Data Dragon Items JSON Response DTOs
// ====================================

/// <summary>
/// Root response structure from Data Dragon item.json endpoint.
/// Contains metadata about the data set and a dictionary of all items.
/// </summary>
public class DdragonItemsResponse
{
    /// <summary>
    /// Type of data returned. Always "item" for item.json.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Patch version of this data set.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Dictionary of items keyed by item ID as string.
    /// Example keys: "3078", "3157", "1001"
    /// </summary>
    [JsonPropertyName("data")]
    public Dictionary<string, DdragonItem> Data { get; set; } = new();
}

/// <summary>
/// Individual item data structure from Data Dragon.
/// Contains all metadata needed to display item information.
/// </summary>
public class DdragonItem
{
    /// <summary>
    /// Localized item display name.
    /// Example (es_ES): "Fuerza de trinidad"
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full HTML description with item stats and effects.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gold cost information for the item.
    /// </summary>
    [JsonPropertyName("gold")]
    public DdragonItemGold Gold { get; set; } = new();

    /// <summary>
    /// Image metadata object containing filename for item icon.
    /// </summary>
    [JsonPropertyName("image")]
    public DdragonImage Image { get; set; } = new();
}

/// <summary>
/// Gold cost structure for items.
/// </summary>
public class DdragonItemGold
{
    /// <summary>
    /// Total gold cost to purchase the item.
    /// Example: 3333 for Trinity Force
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

// ====================================
// Data Dragon Summoner Spells JSON Response DTOs
// ====================================

/// <summary>
/// Root response structure from Data Dragon summoner.json endpoint.
/// Contains metadata and a dictionary of all summoner spells.
/// </summary>
public class DdragonSummonerSpellsResponse
{
    /// <summary>
    /// Type of data returned. Always "summoner" for summoner.json.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Patch version of this data set.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Dictionary of summoner spells keyed by spell key.
    /// Example keys: "SummonerFlash", "SummonerDot", "SummonerTeleport"
    /// </summary>
    [JsonPropertyName("data")]
    public Dictionary<string, DdragonSummonerSpell> Data { get; set; } = new();
}

/// <summary>
/// Individual summoner spell data structure from Data Dragon.
/// </summary>
public class DdragonSummonerSpell
{
    /// <summary>
    /// Numeric spell ID used by Riot API.
    /// Example: "4" for Flash, "14" for Ignite
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Internal spell identifier.
    /// Example: "SummonerFlash", "SummonerDot"
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Localized spell display name.
    /// Example (es_ES): "Destello", "Inflamar"
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Localized spell description.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Spell cooldown in seconds.
    /// Example: 300 for Flash
    /// </summary>
    [JsonPropertyName("cooldown")]
    public List<double> Cooldown { get; set; } = new();

    /// <summary>
    /// Image metadata for spell icon.
    /// </summary>
    [JsonPropertyName("image")]
    public DdragonImage Image { get; set; } = new();
}
