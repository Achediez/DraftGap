// Trabajador en segundo plano que consulta periódicamente los usuarios
// registrados y realiza llamadas a Riot para obtener datos de summoner.
// Propósito:
//  - Ejecutar en background dentro de la aplicación web
//  - Recuperar la lista de usuarios del repositorio
//  - Para cada usuario con un Riot ID (aquí usamos UserName como ejemplo)
//    comprobar si existe el summoner y obtener sus datos mínimos
//  - Actualmente solo hace log de la información; puede extenderse para
//    guardar los datos en la entidad User mediante repo.UpdateAsync

using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using System;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Infrastructure.Riot;
using Microsoft.Extensions.DependencyInjection;

namespace DraftGapBackend.Infrastructure.Riot
{
    // Background worker that periodically checks users with Riot IDs and fetches data
    public class RiotWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;
        // Periodo de ejecución; ajustar según necesidades y límites de la API
        private readonly TimeSpan _period = TimeSpan.FromSeconds(30); // run every 30s

        public RiotWorker(IServiceProvider provider)
        {
            _provider = provider;
        }

        // Método principal del worker; se ejecuta mientras la app esté viva
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _provider.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                    var riot = scope.ServiceProvider.GetRequiredService<IRiotService>();

                    var users = await repo.GetAllAsync();
                    foreach (var user in users)
                    {
                        // Si usamos UserName como Riot ID
                        if (string.IsNullOrWhiteSpace(user.UserName)) continue;
                        var exists = await riot.SummonerExistsAsync(user.UserName);
                        if (exists)
                        {
                            var dto = await riot.GetSummonerByNameAsync(user.UserName);
                            if (dto != null)
                            {
                                // Aquí se podrían mapear campos de dto a user y actualizar
                                // por ejemplo: user.RiotPuuid = dto.puuid; await repo.UpdateAsync(user);
                                Console.WriteLine($"Fetched riot data for {user.UserName}: puuid={dto.puuid} level={dto.summonerLevel}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RiotWorker error: {ex.Message}");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}
