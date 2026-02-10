using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DraftGapBackend.Application.Users;
using DraftGapBackend.Infrastructure.Riot;

namespace DraftGapBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRiotService _riotService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        IRiotService riotService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _riotService = riotService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for: {Email}", request.Email);

            var registerRequest = new RegisterRequest
            {
                Email = request.Email,
                Password = request.Password
            };

            var authResponse = await _userService.RegisterAsync(registerRequest);
            var token = GenerateJwtToken(authResponse.UserId, authResponse.Email, false);

            _logger.LogInformation("✅ User registered successfully: {Email}", request.Email);

            return Ok(new
            {
                token,
                email = authResponse.Email,
                riotId = request.RiotId,
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
            return StatusCode(500, new { error = "Registration failed" });
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

            _logger.LogInformation("✅ User logged in successfully: {Email}, IsAdmin: {IsAdmin}", request.Email, isAdmin);

            return Ok(new
            {
                token,
                email = authResponse.Email,
                riotId = "",
                isAdmin,
                expiresAt = DateTime.UtcNow.AddDays(1)
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("❌ Login failed for {Email}: {Error}", request.Email, ex.Message);
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
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid token" });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

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

    [HttpGet("debug/users")]
    public async Task<IActionResult> DebugGetAllUsers()
    {
        var users = await _userService.GetAllActiveUsersAsync();
        return Ok(users.Select(u => new
        {
            userId = u.UserId,
            email = u.Email,
            passwordHashLength = u.PasswordHash?.Length ?? 0,
            passwordHashPreview = u.PasswordHash?.Substring(0, Math.Min(20, u.PasswordHash.Length)) + "...",
            riotId = u.RiotId,
            createdAt = u.CreatedAt
        }));
    }

    private string GenerateJwtToken(int userId, string email, bool isAdmin)
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
