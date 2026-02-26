using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Dtos.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// Controlador para sincronizaciones iniciadas por el usuario.
/// Endpoints:
/// - POST /api/sync/trigger: Dispara sync manual
/// - GET /api/sync/history: Historial de sincronizaciones
/// Requiere autenticación: Sí (JWT Bearer token)
/// Los jobs creados son procesados por RiotSyncBackgroundService.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly IUserSyncService _userSyncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        IUserSyncService userSyncService,
        ILogger<SyncController> logger)
    {
        _userSyncService = userSyncService;
        _logger = logger;
    }

    /// <summary>
    /// Dispara una sincronización manual de los datos del usuario.
    /// Crea un SyncJob que será procesado en background.
    /// El job actualizará:
    /// - Ranked stats de Riot API
    /// - Match history (nuevas partidas desde último sync)
    /// Estado inicial: PENDING
    /// </summary>
    /// <param name="request">Opciones de sync (forceRefresh para re-sincronizar todo)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <remarks>
    /// El proceso de sync puede tardar 10-30 segundos dependiendo de:
    /// - Cantidad de partidas nuevas
    /// - Rate limits de Riot API
    /// Consulta /api/sync/history para ver el progreso.
    /// </remarks>
    /// <response code="200">Job de sync creado exitosamente</response>
    /// <response code="400">Usuario sin Riot account vinculado</response>
    /// <response code="401">Token inválido</response>
    /// <response code="500">Error al crear job</response>
    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerSync([FromBody] TriggerSyncRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            request ??= new TriggerSyncRequest();
            var userId = GetUserIdFromClaims();

            var job = await _userSyncService.TriggerUserSyncAsync(userId, request, cancellationToken);

            return Ok(job);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Sync trigger failed: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger sync");
            return StatusCode(500, new { error = "Failed to trigger sync" });
        }
    }

    /// <summary>
    /// Get sync history for current user
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetSyncHistory(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var history = await _userSyncService.GetUserSyncHistoryAsync(userId, cancellationToken);

            return Ok(history);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Get sync history failed: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve sync history");
            return StatusCode(500, new { error = "Failed to retrieve sync history" });
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
