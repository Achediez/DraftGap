// Configuración de inyección de dependencias para la infraestructura
// Registra los servicios y repositorios concretos en el contenedor DI
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using DraftGapBackend.Application.Users;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Persistence;
using System;
using DraftGapBackend.Infrastructure.Riot;

namespace DraftGapBackend.Infrastructure
{
    public static class DependencyInjection
    {
        // Método de extensión para registrar servicios de infraestructura
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Cambiado a Singleton para que la lista de usuarios se comparta entre peticiones
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            // Servicio de usuario de aplicación
            services.AddScoped<IUserService, UserService>();

            // Leer configuración de Riot
            var riotApiKey = configuration["Riot:ApiKey"] ?? string.Empty;
            var riotAccountRegion = configuration["Riot:AccountRegion"] ?? "https://europe.api.riotgames.com"; // Account API (regional)
            var riotPlatformBase = configuration["Riot:PlatformBase"] ?? "https://euw1.api.riotgames.com"; // Summoner API (platform-based)

            // Validar que la API key esté configurada
            if (string.IsNullOrWhiteSpace(riotApiKey))
            {
                throw new InvalidOperationException(
                    "Riot API key not configured. Please set 'Riot:ApiKey' in secrets.json or environment variables.");
            }

            // HttpClient para Account API (regional endpoint - Europe para obtener PUUID)
            services.AddHttpClient("RiotAccountClient", client =>
            {
                client.BaseAddress = new Uri(riotAccountRegion);
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("X-Riot-Token", riotApiKey);
            });

            // HttpClient para Summoner API (platform-based endpoint - euw1 para datos de summoner)
            services.AddHttpClient("RiotSummonerClient", client =>
            {
                client.BaseAddress = new Uri(riotPlatformBase);
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("X-Riot-Token", riotApiKey);
            });

            // Registrar servicio de Riot
            services.AddSingleton<IRiotService, RiotService>();
            services.AddHostedService<RiotWorker>();

            // ...otros servicios...
            return services;
        }
    }
}
