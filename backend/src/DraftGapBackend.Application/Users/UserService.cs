// Implementación de la lógica de aplicación para usuarios
// Orquesta la lógica de registro y login usando el repositorio de dominio
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Users;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

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
            // Imprime la lista de usuarios tras registrar
            PrintAllUsers();
            return user;
        }
        // Lógica de login de usuario
        public async Task<User?> LoginAsync(LoginUserRequest request)
        {
            // Imprime la lista de usuarios antes de intentar login
            PrintAllUsers();
            User? user = null;
            if (!string.IsNullOrWhiteSpace(request.EmailOrUserName))
            {
                user = await _userRepository.GetByEmailAsync(request.EmailOrUserName) ??
                       await _userRepository.GetByUserNameAsync(request.EmailOrUserName);
            }
            if (user == null)
            {
                Console.WriteLine($"No se encontró usuario con email/username: {request.EmailOrUserName}");
                return null;
            }
            // Verificar contraseña
            var inputHash = HashPassword(request.Password);
            Console.WriteLine($"Hash esperado: {user.PasswordHash}");
            Console.WriteLine($"Hash recibido: {inputHash}");
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                Console.WriteLine("El hash de la contraseña no coincide");
                return null;
            }
            Console.WriteLine("Login exitoso");
            return user;
        }
        // Hash de contraseña simple 
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
        // Método auxiliar para imprimir todos los usuarios registrados
        private void PrintAllUsers()
        {
            // Usa reflexión para acceder a la lista interna _users si existe
            var usersField = _userRepository.GetType().GetField("_users", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (usersField != null)
            {
                var users = usersField.GetValue(_userRepository) as IEnumerable<User>;
                if (users != null)
                {
                    Console.WriteLine("Usuarios registrados en memoria:");
                    foreach (var u in users)
                    {
                        Console.WriteLine($"Id: {u.Id}, Email: {u.Email}, UserName: {u.UserName}, Hash: {u.PasswordHash}");
                    }
                }
            }
        }
    }
}
