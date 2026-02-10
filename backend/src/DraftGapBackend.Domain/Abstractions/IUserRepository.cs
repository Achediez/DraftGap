using DraftGapBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DraftGapBackend.Domain.Abstractions;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository
{
    // Query operations
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRiotIdAsync(string riotId);
    Task<User?> GetByRiotPuuidAsync(string puuid);
    Task<IEnumerable<User>> GetAllActiveUsersAsync();
    Task<IEnumerable<User>> GetUsersRequiringSyncAsync();

    // Command operations
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int userId);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> RiotIdExistsAsync(string riotId);
}
