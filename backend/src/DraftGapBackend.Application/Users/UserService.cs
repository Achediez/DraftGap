using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory; // Asegúrate de tener esta referencia
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using BC = BCrypt.Net.BCrypt;

namespace DraftGapBackend.Application.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache; // Añadido para el control de intentos

    public UserService(IUserRepository userRepository, IMemoryCache cache)
    {
        _userRepository = userRepository;
        _cache = cache;
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
        string cacheKey = $"login_attempts_{request.EmailOrUserName}";

        // 1. Verificar si ya superó el límite de intentos en memoria
        if (_cache.TryGetValue(cacheKey, out int attempts) && attempts >= 3)
        {
            throw new UnauthorizedAccessException("Demasiados intentos fallidos. Cuenta bloqueada temporalmente (15 min).");
        }

        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.EmailOrUserName);

        // 2. Validar si el usuario existe y la contraseña es correcta
        if (user == null || !BC.Verify(request.Password, user.PasswordHash))
        {
            // Incrementar contador de fallos
            attempts++;
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15)); // El bloqueo dura 15 minutos

            _cache.Set(cacheKey, attempts, cacheOptions);

            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        // 3. Si el login es exitoso, eliminamos el rastro de intentos fallidos
        _cache.Remove(cacheKey);

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
};