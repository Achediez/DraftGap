using DraftGapBackend.Application.Friends;
using DraftGapBackend.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// User search and friends functionality
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
    /// Search for a user by Riot ID
    /// </summary>
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
