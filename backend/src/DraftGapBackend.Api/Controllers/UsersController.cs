// Controlador de usuarios para registro y login
using DraftGapBackend.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace DraftGapBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            // ...llamar a servicio y retornar resultado...
            return Ok();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserRequest request)
        {
            // ...llamar a servicio y retornar resultado...
            return Ok();
        }
    }
}
