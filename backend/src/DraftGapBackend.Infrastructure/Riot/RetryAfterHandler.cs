using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Riot;

/// <summary>
/// DelegatingHandler that retries requests when Riot API responds with 429 (rate limit)
/// or transient 5xx errors. It honors the Retry-After header when present.
/// This avoids introducing an external dependency (Polly) while providing
/// basic retry semantics tuned for Riot API behaviour.
/// </summary>
public class RetryAfterHandler : DelegatingHandler
{
    private readonly int _maxRetries = 3;

    public RetryAfterHandler()
    {
        // Default inner handler will be set by HttpClientFactory
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;

        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            // Send the request
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // If success, return immediately
            if (response.IsSuccessStatusCode)
                return response;

            // Determine if we should retry: 429 or 5xx
            var status = (int)response.StatusCode;
            var shouldRetry = response.StatusCode == (HttpStatusCode)429 || status >= 500;

            if (!shouldRetry || attempt == _maxRetries)
                return response;

            // Try to read Retry-After header
            TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(2, attempt + 1)); // exponential backoff base

            if (response.Headers.RetryAfter != null)
            {
                if (response.Headers.RetryAfter.Delta.HasValue)
                {
                    delay = response.Headers.RetryAfter.Delta.Value;
                }
                else if (response.Headers.RetryAfter.Date.HasValue)
                {
                    var retryDate = response.Headers.RetryAfter.Date.Value;
                    var computed = retryDate - DateTimeOffset.UtcNow;
                    if (computed > TimeSpan.Zero)
                        delay = computed;
                }
            }

            // Dispose the response before retrying to free sockets
            response.Dispose();

            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested
                throw;
            }

            // On retry, the request content (if any) may have been disposed by SendAsync
            // Clone request content when necessary. For safety, recreate the request if it has content.
            if (request.Content != null && attempt < _maxRetries)
            {
                request = await CloneHttpRequestMessageAsync(request).ConfigureAwait(false);
            }
        }

        // Should not reach here, but return the last response if any
        return response!;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        // Copy the request's content (via a MemoryStream) if present
        if (request.Content != null)
        {
            var ms = new System.IO.MemoryStream();
            await request.Content.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            // Copy content headers
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy the request headers
        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        // Note: HttpRequestMessage.Options are not copied intentionally. If your
        // requests rely on Options, extend this method to copy them explicitly.

        return clone;
    }
}
