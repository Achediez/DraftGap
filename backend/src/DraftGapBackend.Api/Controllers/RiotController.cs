using DraftGapBackend.Infrastructure.Riot;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DraftGapBackend.Api.Controllers;

/// <summary>
/// Controlador para pruebas de integración con la API de Riot
/// Proporciona endpoints para verificar si un summoner existe y obtener su información
/// usando Riot ID (gameName#tagLine) con la API v1 moderna de Riot
/// </summary>
[Route("api/riot")]
[ApiController]
public class RiotController : ControllerBase
{
    private readonly IRiotService _riotService;

    public RiotController(IRiotService riotService)
    {
        _riotService = riotService;
    }

    /// <summary>
    /// Verifica si existe un summoner con el Riot ID especificado
    /// </summary>
    /// <param name="riotId">Riot ID en formato gameName#tagLine (ej: "DG Achediez#H10")</param>
    /// <returns>true si el summoner existe, false en caso contrario</returns>
    /// <remarks>
    /// Ejemplo de uso en Swagger:
    /// - Parámetro: riotId = "DG Achediez#H10"
    /// 
    /// Curl desde terminal:
    /// ```bash
    /// curl -X GET "https://localhost:5001/api/riot/summoner-exists?riotId=DG%20Achediez%23H10" \
    ///   -H "accept: application/json" -k
    /// ```
    /// 
    /// PowerShell:
    /// ```powershell
    /// $riotId = "DG Achediez#H10"
    /// $response = Invoke-WebRequest -Uri "https://localhost:5001/api/riot/summoner-exists?riotId=$([System.Web.HttpUtility]::UrlEncode($riotId))" `
    ///   -Method Get -Headers @{"Accept"="application/json"} -SkipCertificateCheck
    /// $response.Content | ConvertFrom-Json
    /// ```
    /// </remarks>
    [HttpGet("summoner-exists")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SummonerExists([FromQuery] string riotId)
    {
        if (string.IsNullOrWhiteSpace(riotId))
            return BadRequest(new { error = "Riot ID no puede estar vacío. Formato: gameName#tagLine (ej: DG Achediez#H10)" });

        if (!riotId.Contains("#"))
            return BadRequest(new { error = "Riot ID debe contener '#'. Formato: gameName#tagLine (ej: DG Achediez#H10)" });

        try
        {
            var exists = await _riotService.SummonerExistsAsync(riotId);
            return Ok(new { riotId, exists });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { error = "Error al verificar summoner", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene la información completa del summoner usando Riot ID
    /// </summary>
    /// <param name="riotId">Riot ID en formato gameName#tagLine (ej: "DG Achediez#H10")</param>
    /// <returns>Información del summoner (id, puuid, nivel, etc.) o null si no existe</returns>
    /// <remarks>
    /// Ejemplo de uso en Swagger:
    /// - Parámetro: riotId = "DG Achediez#H10"
    /// 
    /// Curl desde terminal:
    /// ```bash
    /// curl -X GET "https://localhost:5001/api/riot/summoner?riotId=DG%20Achediez%23H10" \
    ///   -H "accept: application/json" -k
    /// ```
    /// 
    /// PowerShell:
    /// ```powershell
    /// $riotId = "DG Achediez#H10"
    /// $response = Invoke-WebRequest -Uri "https://localhost:5001/api/riot/summoner?riotId=$([System.Web.HttpUtility]::UrlEncode($riotId))" `
    ///   -Method Get -Headers @{"Accept"="application/json"} -SkipCertificateCheck
    /// $response.Content | ConvertFrom-Json | Format-List
    /// ```
    /// 
    /// Respuesta de ejemplo (200 OK):
    /// ```json
    /// {
    ///   "id": "euw1_...",
    ///   "accountId": "euw1_...",
    ///   "puuid": "...",
    ///   "name": "DG Achediez",
    ///   "profileIconId": 5353,
    ///   "revisionDate": 1735689000000,
    ///   "summonerLevel": 30
    /// }
    /// ```
    /// 
    /// Respuesta de ejemplo (404 Not Found):
    /// ```json
    /// {
    ///   "message": "No se encontró summoner con Riot ID: DG Achediez#H10"
    /// }
    /// ```
    /// </remarks>
    [HttpGet("summoner")]
    [ProducesResponseType(typeof(SummonerDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetSummoner([FromQuery] string riotId)
    {
        if (string.IsNullOrWhiteSpace(riotId))
            return BadRequest(new { error = "Riot ID no puede estar vacío. Formato: gameName#tagLine (ej: DG Achediez#H10)" });

        if (!riotId.Contains("#"))
            return BadRequest(new { error = "Riot ID debe contener '#'. Formato: gameName#tagLine (ej: DG Achediez#H10)" });

        try
        {
            var summoner = await _riotService.GetSummonerByNameAsync(riotId);
            
            if (summoner == null)
                return NotFound(new { message = $"No se encontró summoner con Riot ID: {riotId}" });

            return Ok(summoner);
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { error = "Error al obtener información del summoner", details = ex.Message });
        }
    }
}
