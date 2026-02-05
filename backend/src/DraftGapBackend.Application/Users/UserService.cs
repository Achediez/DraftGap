// Implementación de servicios de aplicación
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Users;

namespace DraftGapBackend.Application.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<User> RegisterAsync(RegisterUserRequest request)
        {
            // ...lógica de registro (hash, validaciones)...
            throw new NotImplementedException();
        }
        public async Task<User?> LoginAsync(LoginUserRequest request)
        {
            // ...lógica de login (verificar hash)...
            throw new NotImplementedException();
        }
    }
}
