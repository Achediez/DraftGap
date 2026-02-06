using DraftGapBackend.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

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
        if (string.IsNullOrWhiteSpace(request.EmailOrUserName) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Debes introducir usuario/email y contraseña");
        var user = await _userService.LoginAsync(request);
        if (user == null)
            return Unauthorized("Credenciales incorrectas");
        return Ok(new { message = "Inicio de sesión correcto", user.Id, user.Email, user.UserName });
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Todos los campos son obligatorios");
        try
        {
            var user = await _userService.RegisterAsync(request);
            return Created($"/api/auth/{user.Id}", new { message = "Registro realizado correctamente", user.Id, user.Email, user.UserName });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/auth/me
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok("Funcionalidad de usuario autenticado no implementada");
    }

    // POST: api/auth/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return NoContent();
    }
}
