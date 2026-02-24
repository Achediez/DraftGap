using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Sync;

// Hosted service that polls the database for PENDING sync jobs and processes them sequentially.
// Sequential processing is intentional — it prevents concurrent Riot API calls from
// exceeding the dev key rate limit of 20 requests per second / 100 per 2 minutes.
public class RiotSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RiotSyncBackgroundService> _logger;

    // Interval between polling cycles when no jobs are found.
    private static readonly TimeSpan IdlePollingInterval = TimeSpan.FromSeconds(30);

    // Interval between processing individual jobs to respect rate limits.
    private static readonly TimeSpan JobProcessingDelay = TimeSpan.FromSeconds(2);

    public RiotSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<RiotSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RiotSyncBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await ProcessNextPendingJobAsync(stoppingToken);

                // If no job was found, wait the full idle interval before checking again.
                // If a job was processed, wait a short delay then immediately check for more.
                var delay = processed ? JobProcessingDelay : IdlePollingInterval;
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown — do not log as error.
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in RiotSyncBackgroundService polling loop.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("RiotSyncBackgroundService stopped.");
    }

    private async Task<bool> ProcessNextPendingJobAsync(CancellationToken cancellationToken)
    {
        // Each iteration uses a fresh DI scope because DbContext and scoped services
        // cannot be consumed directly from a singleton-lifetime hosted service.
        await using var scope = _scopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var syncService = scope.ServiceProvider.GetRequiredService<IDataSyncService>();

        // Claim the oldest pending job atomically by updating its status before processing.
        var job = await context.SyncJobs
            .Where(j => j.Status == "PENDING")
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (job == null)
            return false;

        _logger.LogInformation(
            "Processing sync job {JobId} for PUUID {Puuid}.", job.JobId, job.Puuid);


        await syncService.ProcessSyncJobAsync(job, cancellationToken);
        return true;
    }
}
