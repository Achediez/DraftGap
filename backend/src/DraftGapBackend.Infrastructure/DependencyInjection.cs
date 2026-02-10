using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Persistence;
using DraftGapBackend.Infrastructure.Riot;

namespace DraftGapBackend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped<IUserRepository, InMemoryUserRepository>();

        // Register HTTP client for Riot API
        services.AddHttpClient<IRiotService, RiotService>();

        // Register Riot service
        services.AddScoped<IRiotService, RiotService>();

        // TODO: Register background workers when implemented
        // services.AddHostedService<RiotWorker>();

        return services;
    }
}
