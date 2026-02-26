using DraftGapBackend.Application.Common;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Matches;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DraftGapBackend.API.Controllers;

/// <summary>
/// Controlador para historial de partidas y detalles.
/// Endpoints:
/// - GET /api/matches: Lista paginada con filtros opcionales
/// - GET /api/matches/{matchId}: Detalles completos de una partida
/// Requiere autenticación: Sí (JWT Bearer token)
/// Soporta filtros: champion, position, win/loss, queue, fecha
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
    /// Obtiene historial de partidas del usuario con paginación y filtros.
    /// </summary>
    /// <param name="page">Número de página (1-based, default: 1)</param>
    /// <param name="pageSize">Tamaño de página (1-100, default: 10)</param>
    /// <param name="championName">Filtrar por nombre de campeón (ej: "Aatrox")</param>
    /// <param name="teamPosition">Filtrar por posición (TOP, JUNGLE, MIDDLE, BOTTOM, UTILITY)</param>
    /// <param name="win">Filtrar por resultado (true=victorias, false=derrotas)</param>
    /// <param name="queueId">Filtrar por tipo de cola (420=Ranked Solo, 440=Flex, 0=todas)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>
    /// Resultado paginado con:
    /// - items: Array de partidas
    /// - page, pageSize, totalCount: Metadata de paginación
    /// - hasNextPage, hasPreviousPage: Flags de navegación
    /// </returns>
    /// <response code="200">Matches obtenidos exitosamente</response>
    /// <response code="400">Parámetros de paginación o filtros inválidos</response>
    /// <response code="401">Token inválido</response>
    /// <response code="500">Error interno</response>
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
            // Validar parámetros de paginación
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

            // Construir objeto de filtros desde query parameters
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
    /// Obtiene detalles completos de una partida específica.
    /// Incluye:
    /// - Información general de la partida (duración, modo, versión)
    /// - Todos los participantes agrupados por equipo
    /// - Stats detalladas de cada jugador (K/D/A, damage, gold, CS, vision)
    /// - Builds completas (items, runes, summoner spells)
    /// </summary>
    /// <param name="matchId">ID de la partida (formato: REGION_MATCHID, ej: EUW1_123456)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <response code="200">Detalles de la partida obtenidos</response>
    /// <response code="404">Partida no encontrada</response>
    /// <response code="500">Error interno</response>
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
