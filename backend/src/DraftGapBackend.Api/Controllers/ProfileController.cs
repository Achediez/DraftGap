using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Dtos.Profile;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// Controlador para gestión del perfil de usuario.
/// Endpoints:
/// - GET /api/profile: Obtiene perfil completo del usuario autenticado
/// - PUT /api/profile: Actualiza Riot ID y/o región
/// Requiere autenticación: Sí (JWT Bearer token)
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
    /// Obtiene el perfil completo del usuario autenticado.
    /// Incluye:
    /// - Datos de usuario (email, riotId, region, lastSync)
    /// - Datos de summoner (name, level, profileIconId) si está vinculado
    /// - Estado de admin (basado en claims del token)
    /// </summary>
    /// <response code="200">Perfil obtenido exitosamente</response>
    /// <response code="401">Token inválido o expirado</response>
    /// <response code="404">Usuario no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
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
