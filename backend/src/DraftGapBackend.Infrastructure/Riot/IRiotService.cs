// Servicio de integración con la API de Riot (interfaz)
// Este archivo define los métodos que expone el servicio Riot para
// comprobar si existe un summoner y obtener información básica.
// La implementación concreta reside en RiotService.cs.

using System.Threading.Tasks;
using DraftGapBackend.Domain.Users;

namespace DraftGapBackend.Infrastructure.Riot
{
    public interface IRiotService
    {
        Task<bool> SummonerExistsAsync(string summonerName);
        Task<SummonerDto?> GetSummonerByNameAsync(string summonerName);
        // ...otros métodos para partidas, mmr, etc.
    }
}
