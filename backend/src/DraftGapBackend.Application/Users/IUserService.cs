using DraftGapBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DraftGapBackend.Application.Users;

public interface IUserService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByRiotIdAsync(string riotId);
    Task<IEnumerable<User>> GetAllActiveUsersAsync();
    Task UpdateUserAsync(User user);
}
