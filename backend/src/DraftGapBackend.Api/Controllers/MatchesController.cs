using DraftGapBackend.Application.Common;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Matches;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// Match history and details
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;
    private readonly IValidator<PaginationRequest> _paginationValidator;
    private readonly IValidator<MatchFilterRequest> _filterValidator;
    private readonly ILogger<MatchesController> _logger;

    public MatchesController(
        IMatchService matchService,
        IValidator<PaginationRequest> paginationValidator,
        IValidator<MatchFilterRequest> filterValidator,
        ILogger<MatchesController> logger)
    {
        _matchService = matchService;
        _paginationValidator = paginationValidator;
        _filterValidator = filterValidator;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated match history for current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMatches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? championName = null,
        [FromQuery] string? teamPosition = null,
        [FromQuery] bool? win = null,
        [FromQuery] int queueId = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pagination = new PaginationRequest { Page = page, PageSize = pageSize };
            var paginationValidation = await _paginationValidator.ValidateAsync(pagination, cancellationToken);

            if (!paginationValidation.IsValid)
            {
                return BadRequest(new
                {
                    error = "Validation failed",
                    errors = paginationValidation.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var filter = new MatchFilterRequest
            {
                ChampionName = championName,
                TeamPosition = teamPosition,
                Win = win,
                QueueId = queueId
            };

            var filterValidation = await _filterValidator.ValidateAsync(filter, cancellationToken);
            if (!filterValidation.IsValid)
            {
                return BadRequest(new
                {
                    error = "Validation failed",
                    errors = filterValidation.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetUserIdFromClaims();
            var result = await _matchService.GetUserMatchesAsync(userId, pagination, filter, cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Get matches failed: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve matches");
            return StatusCode(500, new { error = "Failed to retrieve matches" });
        }
    }

    /// <summary>
    /// Get detailed information about a specific match
    /// </summary>
    [HttpGet("{matchId}")]
    public async Task<IActionResult> GetMatchDetail(string matchId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var match = await _matchService.GetMatchDetailAsync(matchId, userId, cancellationToken);

            if (match == null)
                return NotFound(new { error = "Match not found" });

            return Ok(match);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve match detail for {MatchId}", matchId);
            return StatusCode(500, new { error = "Failed to retrieve match details" });
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
