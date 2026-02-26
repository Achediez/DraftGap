using DraftGapBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// Controlador para estadísticas de ranked.
/// Endpoints:
/// - GET /api/ranked: Stats de Solo/Duo y Flex queue
/// Requiere autenticación: Sí (JWT Bearer token)
/// Datos actualizados por el sistema de sync automático.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RankedController : ControllerBase
{
    private readonly IRankedService _rankedService;
    private readonly ILogger<RankedController> _logger;

    public RankedController(
        IRankedService rankedService,
        ILogger<RankedController> logger)
    {
        _rankedService = rankedService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene estadísticas de ranked del usuario autenticado.
    /// Retorna stats separadas para:
    /// - soloQueue: RANKED_SOLO_5x5 (Ranked Solo/Duo)
    /// - flexQueue: RANKED_FLEX_SR (Ranked Flex 5v5)
    /// Cada cola incluye: tier, rank, LP, wins, losses, winrate
    /// </summary>
    /// <remarks>
    /// Si el usuario no ha jugado ranked, ambas colas serán null.
    /// Los datos se actualizan durante el proceso de sync.
    /// </remarks>
    /// <response code="200">Stats de ranked obtenidas</response>
    /// <response code="404">Usuario sin datos de ranked (debe hacer sync primero)</response>
    /// <response code="401">Token inválido</response>
    /// <response code="500">Error interno</response>
    [HttpGet]
    public async Task<IActionResult> GetRankedStats(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var stats = await _rankedService.GetUserRankedStatsAsync(userId, cancellationToken);

            if (stats == null)
                return NotFound(new { error = "Ranked stats not found. Please sync your account first." });

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ranked stats");
            return StatusCode(500, new { error = "Failed to retrieve ranked stats" });
        }
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid token");
        return userId;
    }
}
