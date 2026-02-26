using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Profile;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// User profile management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IProfileService profileService,
        IValidator<UpdateProfileRequest> updateProfileValidator,
        ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _updateProfileValidator = updateProfileValidator;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var profile = await _profileService.GetProfileAsync(userId, cancellationToken);

            if (profile == null)
                return NotFound(new { error = "Profile not found" });

            profile.IsAdmin = User.IsInRole("Admin");

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve profile");
            return StatusCode(500, new { error = "Failed to retrieve profile" });
        }
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _updateProfileValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    error = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetUserIdFromClaims();
            var profile = await _profileService.UpdateProfileAsync(userId, request, cancellationToken);

            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Profile update failed: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update profile");
            return StatusCode(500, new { error = "Failed to update profile" });
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
