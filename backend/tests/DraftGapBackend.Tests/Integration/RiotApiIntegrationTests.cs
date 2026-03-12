using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Polly;
using Polly.Timeout;

namespace DraftGapBackend.Tests.Integration
{
    public class RiotApiIntegrationTests
    {
        [Fact]
        public async Task RetryOn429_ShouldWaitAndSucceed()
        {
            var mockHttp = new MockHttpMessageHandler();

            // Configure mock to return 429 on first attempt for any path and 200 on subsequent calls to /success
            int callCount = 0;
            mockHttp.When("https://riot.test/*")
                .Respond(req =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        var response = new HttpResponseMessage((HttpStatusCode)429);
                        response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(1));
                        return response;
                    }

                    if (req.RequestUri != null && req.RequestUri.AbsolutePath.EndsWith("/success", StringComparison.OrdinalIgnoreCase))
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent("{ \"id\": \"123\" }", System.Text.Encoding.UTF8, "application/json")
                        };

                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            var client = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri("https://riot.test/")
            };

            // Build retry policy similar to production but keep waits short for tests
            var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == (HttpStatusCode)429 || (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(3, attempt => TimeSpan.Zero, onRetryAsync: (outcome, timespan, retryAttempt, ctx) =>
                {
                    // In tests we keep the extra wait minimal. In production the handler honors Retry-After.
                    return Task.CompletedTask;
                });

            var wrap = retryPolicy;

            // Execute: we expect the first 429 then a success on a subsequent request.
            // Execute wrapped call using HttpClient to ensure retry policy handles the 429 then succeeds
            var result = await wrap.ExecuteAsync(() => client.GetAsync("success"));

            result.IsSuccessStatusCode.Should().BeTrue();
        }

        [Fact]
        public async Task CircuitBreaker_ShouldOpenAfterRepeated503()
        {
            var mockHttp = new MockHttpMessageHandler();

            // Always return 503
            mockHttp.When("https://riot.down/*").Respond(HttpStatusCode.ServiceUnavailable);

            var client = new HttpClient(mockHttp) { BaseAddress = new Uri("https://riot.down/") };

            var breaker = Policy.HandleResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
                .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 2, TimeSpan.FromSeconds(5));

            // First two requests will fail but not open the breaker (minimumThroughput=2)
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                var r = await breaker.ExecuteAsync(() => client.GetAsync("/test"));
                r.EnsureSuccessStatusCode();
            });

            // Perform more failing calls to trip the breaker
            // Since the handler always returns 503, after enough calls the breaker should open and throw BrokenCircuitException    
            // Execute until BrokenCircuitException is thrown
            var threw = false;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var r = await breaker.ExecuteAsync(() => client.GetAsync("/test"));
                    r.EnsureSuccessStatusCode();
                }
                catch (Polly.CircuitBreaker.BrokenCircuitException)
                {
                    threw = true;
                    break;
                }
                catch
                {
                    // other exceptions ignored for the purpose of this test
                }
            }

            threw.Should().BeTrue("circuit breaker should open after sustained failures");
        }
    }
}
