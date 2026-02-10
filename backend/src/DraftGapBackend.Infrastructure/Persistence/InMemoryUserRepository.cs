using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DraftGapBackend.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of IUserRepository for testing
/// Use Entity Framework repository in production
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public Task<User?> GetByIdAsync(int userId)
    {
        var user = _users.FirstOrDefault(u => u.UserId == userId);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _users.FirstOrDefault(u => u.Email == email);
        return Task.FromResult(user);
    }

    public Task<User?> GetByRiotIdAsync(string riotId)
    {
        var user = _users.FirstOrDefault(u => u.RiotId == riotId);
        return Task.FromResult(user);
    }

    public Task<User?> GetByRiotPuuidAsync(string puuid)
    {
        var user = _users.FirstOrDefault(u => u.RiotPuuid == puuid);
        return Task.FromResult(user);
    }

    public Task<IEnumerable<User>> GetAllActiveUsersAsync()
    {
        var users = _users.Where(u => u.IsActive).ToList();
        return Task.FromResult<IEnumerable<User>>(users);
    }

    public Task<IEnumerable<User>> GetUsersRequiringSyncAsync()
    {
        var users = _users
            .Where(u => u.IsActive && (u.LastSync == null || u.LastSync < DateTime.UtcNow.AddHours(-24)))
            .ToList();
        return Task.FromResult<IEnumerable<User>>(users);
    }

    public Task<User> CreateAsync(User user)
    {
        user.UserId = _nextId++;
        user.CreatedAt = DateTime.UtcNow;
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user)
    {
        var existing = _users.FirstOrDefault(u => u.UserId == user.UserId);
        if (existing != null)
        {
            existing.Email = user.Email;
            existing.PasswordHash = user.PasswordHash;
            existing.RiotId = user.RiotId;
            existing.RiotPuuid = user.RiotPuuid;
            existing.LastSync = user.LastSync;
            existing.IsActive = user.IsActive;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int userId)
    {
        var user = _users.FirstOrDefault(u => u.UserId == userId);
        if (user != null)
        {
            _users.Remove(user);
        }
        return Task.CompletedTask;
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        var exists = _users.Any(u => u.Email == email);
        return Task.FromResult(exists);
    }

    public Task<bool> RiotIdExistsAsync(string riotId)
    {
        var exists = _users.Any(u => u.RiotId == riotId);
        return Task.FromResult(exists);
    }
}
