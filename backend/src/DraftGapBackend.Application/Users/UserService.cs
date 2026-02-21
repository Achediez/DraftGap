using System;
<<<<<<< HEAD
using System.Collections.Generic;
using System.Threading.Tasks;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using BC = BCrypt.Net.BCrypt;
=======
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;
>>>>>>> main

namespace DraftGapBackend.Application.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
<<<<<<< HEAD
        _userRepository = userRepository;
=======
        private readonly IUserRepository _userRepository;
        // Inyección del repositorio de usuarios
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        // Lógica de registro de usuario
        public async Task<User> RegisterAsync(RegisterUserRequest request)
        {
            // Validaciones de seguridad y formato (acumula todos los errores)
            var validationErrors = UserValidator.ValidateAll(request.UserName, request.Email, request.Password);
            if (validationErrors.Any())
            {
                // Muestra todos los errores en consola
                foreach (var err in validationErrors)
                    Console.WriteLine($"[VALIDATION ERROR] {err}");
                // Lanza una excepción con todos los errores concatenados
                throw new ArgumentException(string.Join(" | ", validationErrors));
            }

            await EnsureUserDoesNotExist(request.Email, request.UserName);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.UserName,
                PasswordHash = HashPassword(request.Password)
            };
            await _userRepository.AddAsync(user);
            // Imprime la lista de usuarios tras registrar
            LogAllUsers();
            return user;
        }
        // Lógica de login de usuario
        public async Task<User?> LoginAsync(LoginUserRequest request)
        {
            // Imprime la lista de usuarios antes de intentar login
            LogAllUsers();
            var user = !string.IsNullOrWhiteSpace(request.EmailOrUserName)
                ? await _userRepository.GetByEmailAsync(request.EmailOrUserName) ??
                  await _userRepository.GetByUserNameAsync(request.EmailOrUserName)
                : null;
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
        // Asegura que el usuario no exista ya por email o username
        private async Task EnsureUserDoesNotExist(string email, string userName)
        {
            if (await _userRepository.GetByEmailAsync(email) != null)
                throw new InvalidOperationException("El email ya está registrado");
            if (await _userRepository.GetByUserNameAsync(userName) != null)
                throw new InvalidOperationException("El nombre de usuario ya está registrado");
        }
        // Hash de contraseña simple 
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        // Verifica el hash de la contraseña
        private static bool VerifyPassword(string password, string hash) =>
            HashPassword(password) == hash;
        // Log de usuarios en memoria usando reflexión
        private void LogAllUsers()
        {
            var usersField = _userRepository.GetType().GetField("_users", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (usersField != null)
            {
                var users = usersField.GetValue(_userRepository) as System.Collections.IEnumerable;
                if (users != null)
                {
                    Console.WriteLine("Usuarios registrados en memoria:");
                    foreach (var u in users)
                    {
                        if (u is User user)
                        {
                            Console.WriteLine($"Id: {user.Id}, Email: {user.Email}, UserName: {user.UserName}, Hash: {user.PasswordHash}");
                        }
                    }
                }
            }
        }
>>>>>>> main
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate email doesn't exist
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Hash password
        var passwordHash = BC.HashPassword(request.Password, workFactor: 11);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user);

        return new AuthResponse
        {
            UserId = createdUser.UserId,
            Email = createdUser.Email,
            Message = "Registration successful"
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.EmailOrUserName);

        // Validate user exists and password is correct
        if (user == null || !BC.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is disabled");
        }

        return new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            Message = "Login successful"
        };
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<IEnumerable<User>> GetAllActiveUsersAsync()
    {
        return await _userRepository.GetAllActiveUsersAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        await _userRepository.UpdateAsync(user);
    }

    public async Task<User?> GetUserByRiotIdAsync(string riotId)
    {
        return await _userRepository.GetByRiotIdAsync(riotId);
    }

}