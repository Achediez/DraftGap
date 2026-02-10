using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using BC = BCrypt.Net.BCrypt;

namespace DraftGapBackend.Application.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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

    public async Task<User?> GetUserByIdAsync(int userId)
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