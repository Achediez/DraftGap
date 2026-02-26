using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DraftGapBackend.Application.Users;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using DraftGapBackend.Infrastructure.Riot;

namespace DraftGapBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controlador para autenticación y registro de usuarios.
/// Endpoints:
/// - POST /api/auth/register: Registro de nuevo usuario
/// - POST /api/auth/login: Login con credenciales
/// - POST /api/auth/refresh: Renovar tokens usando refresh token
/// Sistema de autenticación basado en JWT:
/// - accessToken: Expira en 1 hora, usado en Authorization header
/// - refreshToken: Expira en 7 días, usado para renovar accessToken
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRiotService _riotService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly ApplicationDbContext _context;

    public AuthController(
        IUserService userService,
        IRiotService riotService,
        IConfiguration configuration,
        ILogger<AuthController> logger,
        ApplicationDbContext context)
    {
        _userService = userService;
        _riotService = riotService;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Registra un nuevo usuario en la plataforma.
    /// Proceso:
    /// 1. Valida email y password
    /// 2. Verifica que el email no esté registrado
    /// 3. Valida Riot ID contra Riot API
    /// 4. Hashea password con BCrypt
    /// 5. Crea usuario en BD
    /// 6. Genera tokens JWT
    /// </summary>
    /// <param name="request">
    /// - email: Email válido
    /// - password: Mínimo 6 caracteres
    /// - riotId: GameName#TAG
    /// - region: platform ID (euw1, na1, etc.)
    /// </param>
    /// <response code="200">Usuario registrado exitosamente, retorna tokens</response>
    /// <response code="400">Email ya existe o Riot ID inválido</response>
    /// <response code="500">Error interno</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for: {Email} with Riot ID: {RiotId}", request.Email, request.RiotId);

            var riotIdParts = request.RiotId.Split('#');
            if (riotIdParts.Length != 2)
                return BadRequest(new { error = "Invalid Riot ID format. Use: GameName#TAG" });

            var gameName = riotIdParts[0];
            var tagLine = riotIdParts[1];

            _logger.LogInformation("Verifying Riot account: {GameName}#{TagLine}", gameName, tagLine);
            var riotAccount = await _riotService.GetAccountByRiotIdAsync(gameName, tagLine, request.Region);
            if (riotAccount == null)
            {
                _logger.LogWarning("Riot account not found: {GameName}#{TagLine}", gameName, tagLine);
                return BadRequest(new { error = "Riot account not found. Please check your Riot ID and region." });
            }

            _logger.LogInformation("Riot account verified: PUUID = {Puuid}", riotAccount.Puuid);

            var existingUser = await _userService.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest(new { error = "Email already registered" });

            var existingRiotUser = await _userService.GetUserByRiotIdAsync(request.RiotId);
            if (existingRiotUser != null)
                return BadRequest(new { error = "Riot ID already registered to another account" });

            var registerRequest = new RegisterRequest
            {
                Email = request.Email,
                Password = request.Password
            };
            var authResponse = await _userService.RegisterAsync(registerRequest);

            // Persist Riot identity fields onto the newly created user.
            var user = await _userService.GetUserByIdAsync(authResponse.UserId);
            if (user != null)
            {
                user.RiotId = request.RiotId;
                user.RiotPuuid = riotAccount.Puuid;
                user.Region = request.Region;
                await _userService.UpdateUserAsync(user);
                _logger.LogInformation("Riot data saved: RiotId={RiotId}, Puuid={Puuid}", user.RiotId, user.RiotPuuid);
            }

            // Create the Player row that sync_jobs and player_ranked_stats FK-reference.
            // Without this row, any future sync job for this user will fail with a constraint violation.
            var playerExists = await _context.Players
                .AnyAsync(p => p.Puuid == riotAccount.Puuid);

            if (!playerExists)
            {
                _context.Players.Add(new Player
                {
                    Puuid = riotAccount.Puuid,
                    Region = request.Region
                });
                await _context.SaveChangesAsync();
                _logger.LogInformation("Player record created for PUUID {Puuid}", riotAccount.Puuid);
            }

            var token = GenerateJwtToken(authResponse.UserId, authResponse.Email, false);
            _logger.LogInformation("User registered successfully: {Email} - {RiotId}", request.Email, request.RiotId);

            return Ok(new
            {
                token,
                email = authResponse.Email,
                riotId = request.RiotId,
                puuid = riotAccount.Puuid,
                region = request.Region,
                isAdmin = false,
                expiresAt = DateTime.UtcNow.AddDays(1)
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed for {Email}: {Error}", request.Email, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error for {Email}", request.Email);
            return StatusCode(500, new { error = "Registration failed. Check if Riot API key is configured." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            _logger.LogInformation("Login attempt for: {Email}", request.Email);

            var loginRequest = new LoginRequest
            {
                EmailOrUserName = request.Email,
                Password = request.Password
            };

            var authResponse = await _userService.LoginAsync(loginRequest);

            var adminEmails = _configuration.GetSection("Admin:AllowedEmails").Get<List<string>>() ?? new List<string>();
            var isAdmin = adminEmails.Contains(authResponse.Email);
            var token = GenerateJwtToken(authResponse.UserId, authResponse.Email, isAdmin);

            // Fetch the full user to return the riotId — the login response from IUserService
            // does not include Riot fields, so we load them here for the frontend.
            var user = await _userService.GetUserByIdAsync(authResponse.UserId);
            var riotId = user?.RiotId ?? "";

            _logger.LogInformation("User logged in successfully: {Email}, IsAdmin: {IsAdmin}", request.Email, isAdmin);

            return Ok(new
            {
                token,
                email = authResponse.Email,
                riotId,
                isAdmin,
                expiresAt = DateTime.UtcNow.AddDays(1)
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for {Email}: {Error}", request.Email, ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Email}", request.Email);
            return StatusCode(500, new { error = "Login failed" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { error = "Invalid token" });

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });

            return Ok(new
            {
                email = user.Email,
                riotId = user.RiotId ?? "",
                isAdmin = User.IsInRole("Admin"),
                lastSync = user.LastSync,
                createdAt = user.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current user");
            return StatusCode(500, new { error = "Failed to retrieve user" });
        }
    }

    /// <summary>
    /// Lista todos los usuarios registrados (solo Admin).
    /// Incluye información pública: userId, email, riotId, region, lastSync, isAdmin.
    /// </summary>
    /// <response code="200">Lista de usuarios obtenida</response>
    /// <response code="401">Token inválido</response>
    /// <response code="403">Usuario no tiene rol Admin</response>
    /// <response code="500">Error interno</response>
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllActiveUsersAsync();

            // Obtén la lista de emails de admin desde la configuración
            var adminEmails = _configuration.GetSection("Admin:AllowedEmails").Get<List<string>>() ?? new List<string>();

            return Ok(users.Select(u => new
            {
                userId = u.UserId,
                email = u.Email,
                riotId = u.RiotId,
                region = u.Region,
                lastSync = u.LastSync,
                hasPuuid = !string.IsNullOrEmpty(u.RiotPuuid),
                isAdmin = adminEmails.Contains(u.Email),
                createdAt = u.CreatedAt // Elimina esta línea si no existe en tu entidad User
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user list.");
            return StatusCode(500, new { error = "Failed to retrieve users." });
        }
    }

    private string GenerateJwtToken(Guid userId, string email, bool isAdmin)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RiotId { get; set; } = string.Empty;
    public string Region { get; set; } = "euw1";
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
