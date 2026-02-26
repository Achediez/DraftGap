using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// User-initiated sync operations
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
    /// Trigger manual sync for current user
    /// </summary>
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
