using DraftGapBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// Controlador para el dashboard principal del usuario.
/// Endpoints:
/// - GET /api/dashboard/summary: Resumen completo (ranked, matches, performance, top champions)
/// Requiere autenticación: Sí (JWT Bearer token)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene resumen completo del dashboard del usuario.
    /// Retorna:
    /// - rankedOverview: Stats de Solo/Duo y Flex (tier, rank, LP, winrate)
    /// - recentMatches: Últimas 10 partidas con resultado y KDA
    /// - performanceStats: Promedios de K/D/A y winrate (basado en últimas 20 partidas)
    /// - topChampions: Top 5 campeones más jugados (basado en últimas 50 partidas)
    /// </summary>
    /// <remarks>
    /// Este endpoint es el más usado en la aplicación.
    /// Considera implementar caching para mejorar rendimiento.
    /// </remarks>
    /// <response code="200">Dashboard obtenido exitosamente</response>
    /// <response code="400">Usuario sin cuenta de Riot vinculada</response>
    /// <response code="401">Token inválido</response>
    /// <response code="500">Error interno</response>
    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var summary = await _dashboardService.GetDashboardSummaryAsync(userId, cancellationToken);

            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Dashboard summary failed: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve dashboard summary");
            return StatusCode(500, new { error = "Failed to retrieve dashboard summary" });
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
