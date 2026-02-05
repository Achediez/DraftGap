// Controlador de usuarios para registro y login
// Expone endpoints HTTP para registrar y autenticar usuarios
// Utiliza IUserService para la lógica de aplicación
using DraftGapBackend.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace DraftGapBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        // Inyección de dependencias del servicio de usuario
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        // Endpoint para registrar un usuario
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            // Aquí se llamará a la lógica de registro
            return Ok();
        }
        // Endpoint para login de usuario
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserRequest request)
        {
            // Aquí se llamará a la lógica de login
            return Ok();
        }
    }
}
