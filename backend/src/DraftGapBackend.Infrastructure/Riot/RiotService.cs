// Servicio para integración con la API de Riot
// Este archivo contiene:
// - DTOs para respuestas de la API de Riot (Account API v1)
// - RiotService: implementación que realiza llamadas HTTP usando IHttpClientFactory
//   Usa dos clientes: uno para Account API (regional) y otro para Summoner API (platform-based)
// Nota: los HttpClients se registran en DependencyInjection

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Riot
{
    // DTO para la respuesta de Account API (obtener PUUID)
    public class AccountDto
    {
        public string puuid { get; set; } = string.Empty;
        public string gameName { get; set; } = string.Empty;
        public string tagLine { get; set; } = string.Empty;
    }

    // DTO para la respuesta de Summoner API (obtener datos del summoner)
    public class SummonerDto
    {
        public string id { get; set; } = string.Empty;
        public string accountId { get; set; } = string.Empty;
        public string puuid { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public int profileIconId { get; set; }
        public long revisionDate { get; set; }
        public int summonerLevel { get; set; }
    }

    /// <summary>
    /// Servicio que realiza llamadas a la API de Riot usando IHttpClientFactory
    /// - Usa Account API v1 (regional) para obtener PUUID por Riot ID (gameName + tagLine)
    /// - Usa Summoner API v4 (platform-based) para obtener datos del summoner por PUUID
    /// - El header X-Riot-Token se envía automáticamente en ambos clientes
    /// </summary>
    public class RiotService : IRiotService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RiotService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Comprueba si existe un summoner con el Riot ID indicado (gameName + tagLine)
        /// </summary>
        /// <param name="riotId">Riot ID en formato "gameName#tagLine" ej: "DG Achediez#H10"</param>
        /// <returns>true si existe, false en caso contrario</returns>
        public async Task<bool> SummonerExistsAsync(string riotId)
        {
            if (string.IsNullOrWhiteSpace(riotId)) return false;

            try
            {
                var summoner = await GetSummonerByRiotIdAsync(riotId).ConfigureAwait(false);
                return summoner != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene la información completa del summoner usando Riot ID (gameName + tagLine)
        /// </summary>
        /// <param name="riotId">Riot ID en formato "gameName#tagLine" ej: "DG Achediez#H10"</param>
        /// <returns>SummonerDto con la información completa o null si no existe</returns>
        public async Task<SummonerDto?> GetSummonerByNameAsync(string riotId)
        {
            return await GetSummonerByRiotIdAsync(riotId).ConfigureAwait(false);
        }

        /// <summary>
        /// Método interno que orquesta la obtención de datos:
        /// 1. Convierte Riot ID (gameName#tagLine) en gameName y tagLine separados
        /// 2. Llama a Account API para obtener PUUID
        /// 3. Llama a Summoner API con el PUUID para obtener datos completos
        /// </summary>
        private async Task<SummonerDto?> GetSummonerByRiotIdAsync(string riotId)
        {
            if (string.IsNullOrWhiteSpace(riotId)) return null;

            try
            {
                // Parse Riot ID: "gameName#tagLine"
                var parts = riotId.Split('#');
                if (parts.Length != 2)
                    return null;

                var gameName = parts[0].Trim();
                var tagLine = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(gameName) || string.IsNullOrWhiteSpace(tagLine))
                    return null;

                // Step 1: Get PUUID from Account API (regional endpoint)
                var accountClient = _httpClientFactory.CreateClient("RiotAccountClient");
                var accountUrl = $"/riot/account/v1/accounts/by-riot-id/{Uri.EscapeDataString(gameName)}/{Uri.EscapeDataString(tagLine)}";
                var accountDto = await accountClient.GetFromJsonAsync<AccountDto>(accountUrl).ConfigureAwait(false);

                if (accountDto == null || string.IsNullOrWhiteSpace(accountDto.puuid))
                    return null;

                // Step 2: Get Summoner data from Summoner API (platform-based endpoint) using PUUID
                var summonerClient = _httpClientFactory.CreateClient("RiotSummonerClient");
                var summonerUrl = $"/lol/summoner/v4/summoners/by-puuid/{Uri.EscapeDataString(accountDto.puuid)}";
                var summonerDto = await summonerClient.GetFromJsonAsync<SummonerDto>(summonerUrl).ConfigureAwait(false);

                return summonerDto;
            }
            catch
            {
                return null;
            }
        }
    }
}
