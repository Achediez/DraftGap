using DraftGapBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// Champion data and statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChampionsController : ControllerBase
{
    private readonly IChampionService _championService;
    private readonly ILogger<ChampionsController> _logger;

    public ChampionsController(
        IChampionService championService,
        ILogger<ChampionsController> logger)
    {
        _championService = championService;
        _logger = logger;
    }

    /// <summary>
    /// Get all champions static data
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllChampions(CancellationToken cancellationToken)
    {
        try
        {
            var champions = await _championService.GetAllChampionsAsync(cancellationToken);
            return Ok(champions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve champions");
            return StatusCode(500, new { error = "Failed to retrieve champions" });
        }
    }

    /// <summary>
    /// Get specific champion by ID
    /// </summary>
    [HttpGet("{championId}")]
    public async Task<IActionResult> GetChampionById(int championId, CancellationToken cancellationToken)
    {
        try
        {
            var champion = await _championService.GetChampionByIdAsync(championId, cancellationToken);
            if (champion == null)
                return NotFound(new { error = "Champion not found" });

            return Ok(champion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve champion {ChampionId}", championId);
            return StatusCode(500, new { error = "Failed to retrieve champion" });
        }
    }

    /// <summary>
    /// Get current user's champion statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetUserChampionStats(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var stats = await _championService.GetUserChampionStatsAsync(userId, cancellationToken);

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve champion stats");
            return StatusCode(500, new { error = "Failed to retrieve champion stats" });
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
