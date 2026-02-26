using DraftGapBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// Ranked statistics endpoints
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
    /// Get current user's ranked statistics
    /// </summary>
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
