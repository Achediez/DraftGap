using DraftGapBackend.Application.Friends;
using DraftGapBackend.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// Controlador para búsqueda de usuarios y sistema de amigos.
/// Endpoints:
/// - POST /api/friends/search: Buscar usuario por Riot ID
/// Requiere autenticación: Sí (JWT Bearer token)
/// Funcionalidad base para futura implementación de lista de amigos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IFriendsService _friendsService;
    private readonly IValidator<SearchUserRequest> _searchValidator;
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(
        IFriendsService friendsService,
        IValidator<SearchUserRequest> searchValidator,
        ILogger<FriendsController> logger)
    {
        _friendsService = friendsService;
        _searchValidator = searchValidator;
        _logger = logger;
    }

    /// <summary>
    /// Busca un usuario registrado en la plataforma por su Riot ID.
    /// Útil para:
    /// - Comparar stats con otros jugadores
    /// - Agregar amigos (futura funcionalidad)
    /// - Verificar si un jugador está en la plataforma
    /// </summary>
    /// <param name="request">Riot ID en formato GameName#TAG</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>
    /// Información pública del usuario:
    /// - userId, riotId, region
    /// - summonerName, level, profileIconId
    /// - isActive
    /// </returns>
    /// <response code="200">Usuario encontrado</response>
    /// <response code="400">Formato de Riot ID inválido</response>
    /// <response code="404">Usuario no registrado en la plataforma</response>
    /// <response code="401">Token inválido</response>
    /// <response code="500">Error interno</response>
    [HttpPost("search")]
    public async Task<IActionResult> SearchUser([FromBody] SearchUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _searchValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    error = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var result = await _friendsService.SearchUserByRiotIdAsync(request.RiotId, cancellationToken);

            if (result == null)
                return NotFound(new { error = "User not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search user");
            return StatusCode(500, new { error = "Failed to search user" });
        }
    }
}
