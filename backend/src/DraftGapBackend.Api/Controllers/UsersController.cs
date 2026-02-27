using DraftGapBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// Controlador para búsqueda y consulta de usuarios públicos.
/// Endpoints:
/// - GET /api/users/by-riot-id/{riotId}: Buscar usuario por Riot ID con datos agregados
/// Requiere autenticación: Opcional (depende de política de visibilidad)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IFriendsService _friendsService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IFriendsService friendsService,
        ILogger<UsersController> logger)
    {
        _friendsService = friendsService;
        _logger = logger;
    }

    /// <summary>
    /// Busca un usuario por su Riot ID y devuelve datos agregados.
    /// Incluye:
    /// - Información básica: userId, email, riotId, region
    /// - Summoner: name, level, profileIconId (si está vinculado)
    /// - RankedOverview: stats de Solo/Duo y Flex (si tiene datos)
    /// - RecentMatches: últimas 10 partidas (array vacío si no hay)
    /// - TopChampions: top 5 más jugados (array vacío si no hay)
    /// </summary>
    /// <param name="riotId">Riot ID en formato GameName#TAG (URL-encoded)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <remarks>
    /// Ejemplo de uso:
    /// GET /api/users/by-riot-id/Faker%23KR1
    /// 
    /// Formato requerido: GameName#TAG
    /// - Debe contener exactamente un #
    /// - GameName no puede estar vacío
    /// - TAG no puede estar vacío
    /// 
    /// La búsqueda es case-insensitive.
    /// Solo consulta base de datos, NO llama a Riot API.
    /// </remarks>
    /// <response code="200">Usuario encontrado con datos agregados</response>
    /// <response code="400">Formato de Riot ID inválido</response>
    /// <response code="404">Usuario no encontrado en la plataforma</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("by-riot-id/{riotId}")]
    public async Task<IActionResult> GetUserByRiotId(string riotId, CancellationToken cancellationToken)
    {
        try
        {
            // Validación estricta de formato Riot ID
            if (string.IsNullOrWhiteSpace(riotId))
            {
                _logger.LogWarning("Invalid request: Riot ID is empty or whitespace");
                return BadRequest(new { error = "Riot ID is required" });
            }

            var parts = riotId.Split('#');
            if (parts.Length != 2)
            {
                _logger.LogWarning("Invalid Riot ID format: {RiotId} - Must contain exactly one #", riotId);
                return BadRequest(new { error = "Invalid Riot ID format. Must be: GameName#TAG" });
            }

            var gameName = parts[0];
            var tagLine = parts[1];

            if (string.IsNullOrWhiteSpace(gameName))
            {
                _logger.LogWarning("Invalid Riot ID format: {RiotId} - GameName is empty", riotId);
                return BadRequest(new { error = "GameName cannot be empty" });
            }

            if (string.IsNullOrWhiteSpace(tagLine))
            {
                _logger.LogWarning("Invalid Riot ID format: {RiotId} - TagLine is empty", riotId);
                return BadRequest(new { error = "TagLine cannot be empty" });
            }

            _logger.LogInformation("Searching user by Riot ID: {RiotId}", riotId);

            // Buscar usuario con datos agregados (solo BD, sin Riot API)
            var userDetails = await _friendsService.GetUserDetailsByRiotIdAsync(riotId, cancellationToken);

            if (userDetails == null)
            {
                _logger.LogWarning("User not found with Riot ID: {RiotId}", riotId);
                return NotFound(new { error = $"User with Riot ID '{riotId}' not found in the platform" });
            }

            _logger.LogInformation("User found: {RiotId} - UserId: {UserId}", riotId, userDetails.UserId);
            return Ok(userDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user details for Riot ID: {RiotId}", riotId);
            return StatusCode(500, new { error = "An error occurred while retrieving user details" });
        }
    }
}
