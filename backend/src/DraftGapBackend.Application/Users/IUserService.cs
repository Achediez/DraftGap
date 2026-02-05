// Define los modelos de request y la interfaz de servicio de usuario
// Casos de uso de aplicación: registro y login
using DraftGapBackend.Domain.Users;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Users
{
    // Modelo para registrar usuario
    public class RegisterUserRequest
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    // Modelo para login de usuario
    public class LoginUserRequest
    {
        public string EmailOrUserName { get; set; }
        public string Password { get; set; }
    }

    // Interfaz de servicio de usuario: define los casos de uso
    public interface IUserService
    {
        Task<User> RegisterAsync(RegisterUserRequest request);
        Task<User?> LoginAsync(LoginUserRequest request);
    }
}
