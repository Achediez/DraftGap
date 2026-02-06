// Implementación de la lógica de aplicación para usuarios
// Orquesta la lógica de registro y login usando el repositorio de dominio
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Users;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography;
using System.Text;

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
        // Lógica de registro de usuario
        public async Task<User> RegisterAsync(RegisterUserRequest request)
        {
            // Validar si el email o username ya existen
            var existingByEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingByEmail != null)
                throw new InvalidOperationException("El email ya está registrado");
            var existingByUserName = await _userRepository.GetByUserNameAsync(request.UserName);
            if (existingByUserName != null)
                throw new InvalidOperationException("El nombre de usuario ya está registrado");
            // Hashear la contraseña
            var passwordHash = HashPassword(request.Password);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.UserName,
                PasswordHash = passwordHash
            };
            await _userRepository.AddAsync(user);
            return user;
        }
        // Lógica de login de usuario
        public async Task<User?> LoginAsync(LoginUserRequest request)
        {
            // Buscar usuario por email o username
            User? user = null;
            if (!string.IsNullOrWhiteSpace(request.EmailOrUserName))
            {
                user = await _userRepository.GetByEmailAsync(request.EmailOrUserName) ??
                       await _userRepository.GetByUserNameAsync(request.EmailOrUserName);
            }
            if (user == null)
                return null;
            // Verificar contraseña
            if (!VerifyPassword(request.Password, user.PasswordHash))
                return null;
            return user;
        }
        // Hash de contraseña simple (usa un algoritmo más seguro en producción)
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        // Verifica el hash de la contraseña
        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
