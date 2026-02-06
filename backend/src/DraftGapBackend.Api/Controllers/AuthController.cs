using DraftGapBackend.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DraftGapBackend.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        var user = await _userService.LoginAsync(request);
        if (user == null)
            return Unauthorized("Credenciales incorrectas");
        // Aquí podrías emitir un JWT o establecer una cookie si implementas autenticación real
        return Ok(new { user.Id, user.Email, user.UserName });
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        // Validación básica
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Todos los campos son obligatorios");
        // Intentar registrar
        var user = await _userService.RegisterAsync(request);
        return Created($"/api/auth/{user.Id}", new { user.Id, user.Email, user.UserName });
    }

    // GET: api/auth/me
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        // En una implementación real, aquí se obtendría el usuario autenticado desde el token
        // Por ahora, solo retorna un mensaje de ejemplo
        return Ok("Funcionalidad de usuario autenticado no implementada");
    }

    // POST: api/auth/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Aquí eliminarías cookies o tokens si implementas autenticación real
        return NoContent();
    }
}
