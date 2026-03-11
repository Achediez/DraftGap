using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Riot;

/// <summary>
/// DelegatingHandler that performs a proactive rate-limit check using Redis.
/// Uses a sorted set with timestamps to keep a sliding-window count of recent requests.
/// If the configured limit is exceeded the handler short-circuits with a 429 response
/// to avoid calling the external Riot API.
/// </summary>
public class RedisRateLimitHandler : DelegatingHandler
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimitHandler> _logger;
    private readonly int _maxRequests;
    private readonly int _windowSeconds;
    private readonly string _apiKey;

    public RedisRateLimitHandler(IConnectionMultiplexer redis, IConfiguration configuration, ILogger<RedisRateLimitHandler> logger)
    {
        _redis = redis;
        _logger = logger;
        _maxRequests = int.TryParse(configuration["RiotApi:RateLimit:MaxRequests"], out var mr) ? mr : 90;
        _windowSeconds = int.TryParse(configuration["RiotApi:RateLimit:WindowSeconds"], out var ws) ? ws : 120;
        _apiKey = configuration["RiotApi:ApiKey"] ?? "default";
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"riot:requests:{_apiKey}";
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var windowStart = now - (_windowSeconds * 1000);

            // Add this request timestamp
            var member = Guid.NewGuid().ToString();
            await db.SortedSetAddAsync(key, member, now).ConfigureAwait(false);

            // Remove timestamps older than window
            await db.SortedSetRemoveRangeByScoreAsync(key, 0, windowStart).ConfigureAwait(false);

            // Count requests in window
            var count = await db.SortedSetLengthAsync(key).ConfigureAwait(false);

            // Ensure key has expiration slightly longer than window
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(_windowSeconds + 10)).ConfigureAwait(false);

            if (count > _maxRequests)
            {
                _logger.LogWarning("Redis rate limit reached for Riot API (count={Count}, max={Max}). Short-circuiting request.", count, _maxRequests);

                var retryAfter = TimeSpan.FromSeconds(5);
                var response = new HttpResponseMessage((HttpStatusCode)429)
                {
                    ReasonPhrase = "Rate limit exceeded (local Redis pre-check)"
                };
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(retryAfter);
                return response;
            }
        }
        catch (Exception ex)
        {
            // On Redis failures we should not stop, simply log and continue to attempt the request
            _logger.LogWarning(ex, "Redis rate-limit pre-check failed, continuing without pre-check");
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
