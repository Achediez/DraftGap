// Implementación de la lógica de aplicación para usuarios
// Orquesta la lógica de registro y login usando el repositorio de dominio
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Users;
using System.Threading.Tasks;
using System;

namespace DraftGapBackend.Application.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        // Inyección del repositorio de usuarios
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        // Lógica de registro de usuario (a implementar)
        public async Task<User> RegisterAsync(RegisterUserRequest request)
        {
            // Aquí se implementará la lógica de registro (validaciones, hash, etc.)
            throw new NotImplementedException();
        }
        // Lógica de login de usuario (a implementar)
        public async Task<User?> LoginAsync(LoginUserRequest request)
        {
            // Aquí se implementará la lógica de login (verificar hash, etc.)
            throw new NotImplementedException();
        }
    }
}
