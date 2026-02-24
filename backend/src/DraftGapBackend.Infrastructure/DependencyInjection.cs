using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Persistence;
using DraftGapBackend.Infrastructure.Riot;
using DraftGapBackend.Infrastructure.Sync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DraftGapBackend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Repository layer — scoped per HTTP request.
        services.AddScoped<IUserRepository, UserRepository>();

        // Riot API HTTP client with base timeout.
        services.AddHttpClient<IRiotService, RiotService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "DraftGap/1.0");
        });

        // Riot service — scoped so it shares the HTTP request lifetime.
        services.AddScoped<IRiotService, RiotService>();

        // Data sync service — scoped because it uses ApplicationDbContext which is also scoped.
        services.AddScoped<IDataSyncService, DataSyncService>();

        // Background worker — singleton lifetime managed by the host.
        // Creates its own DI scope internally when processing jobs to safely
        // resolve scoped services (ApplicationDbContext, IDataSyncService).
        services.AddHostedService<RiotSyncBackgroundService>();

        return services;
    }
}
