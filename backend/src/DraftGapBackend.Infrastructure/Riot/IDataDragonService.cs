using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Riot;

/// <summary>
/// Service contract for synchronizing static game data from Riot's Data Dragon CDN.
/// Data Dragon provides champion, item, rune, and other game metadata that changes only with patches.
/// This service fetches and caches this data locally to avoid repeated external calls.
/// </summary>
public interface IDataDragonService
{
    /// <summary>
    /// Fetches all champion data from Data Dragon and stores it in the database.
    /// This operation is idempotent - if champions already exist, sync is skipped.
    /// Champion data includes: ID, name, title, icon URL, and patch version.
    /// Data is fetched in Spanish (es_ES) locale.
    /// </summary>
    /// <param name="ct">Cancellation token for async operation control.</param>
    /// <returns>Task representing the asynchronous sync operation.</returns>
    Task SyncChampionsAsync(CancellationToken ct = default);
    Task SyncItemsAsync(CancellationToken ct = default);
    Task SyncSummonerSpellsAsync(CancellationToken ct = default);

    /// <summary>
    /// Fetches all rune path and rune data from Data Dragon and stores it in the database.
    /// This operation is idempotent - if rune paths already exist, sync is skipped.
    /// Inserts rune paths first, then individual runes to satisfy the FK constraint.
    /// Data is fetched in Spanish (es_ES) locale.
    /// </summary>
    /// <param name="ct">Cancellation token for async operation control.</param>
    /// <returns>Task representing the asynchronous sync operation.</returns>
    Task SyncRunesAsync(CancellationToken ct = default);

}
