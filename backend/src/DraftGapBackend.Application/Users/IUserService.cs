// Casos de uso y lógica de aplicación
using DraftGapBackend.Domain.Users;

namespace DraftGapBackend.Application.Users
{
    public class RegisterUserRequest
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class LoginUserRequest
    {
        public string EmailOrUserName { get; set; }
        public string Password { get; set; }
    }

    public interface IUserService
    {
        Task<User> RegisterAsync(RegisterUserRequest request);
        Task<User?> LoginAsync(LoginUserRequest request);
    }
}
