using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DraftGapBackend.Infrastructure.Persistence;

/// <summary>
/// Repository for <see cref="User"/> entity persistence operations.
/// Encapsulates EF Core access to the application's users table and provides
/// common queries used by application services.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Creates a new instance of <see cref="UserRepository"/>.
    /// </summary>
    /// <param name="context">EF Core database context.</param>
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a user by its unique identifier.
    /// </summary>
    /// <param name="userId">User identifier (GUID).</param>
    /// <returns>The <see cref="User"/> if found; otherwise null.</returns>
    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    /// <summary>
    /// Finds a user by email address.
    /// </summary>
    /// <param name="email">Email to search for.</param>
    /// <returns>The <see cref="User"/> if found; otherwise null.</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Finds a user by their Riot ID (case-insensitive).
    /// </summary>
    /// <param name="riotId">Riot ID in format GameName#TAG.</param>
    /// <returns>The <see cref="User"/> if found; otherwise null.</returns>
    public async Task<User?> GetByRiotIdAsync(string riotId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RiotId != null && u.RiotId.ToLower() == riotId.ToLower());
    }

    /// <summary>
    /// Finds a user by their Riot PUUID.
    /// </summary>
    /// <param name="puuid">Riot PUUID.</param>
    /// <returns>The <see cref="User"/> if found; otherwise null.</returns>
    public async Task<User?> GetByRiotPuuidAsync(string puuid)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RiotPuuid == puuid);
    }

    /// <summary>
    /// Returns all active users.
    /// </summary>
    /// <returns>Collection of active <see cref="User"/>.</returns>
    public async Task<IEnumerable<User>> GetAllActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Returns users that require a data sync (not synced in the last 24 hours).
    /// </summary>
    /// <returns>Collection of users needing synchronization.</returns>
    public async Task<IEnumerable<User>> GetUsersRequiringSyncAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive &&
                   (u.LastSync == null || u.LastSync < DateTime.UtcNow.AddHours(-24)))
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new user record in the database.
    /// </summary>
    /// <param name="user">User entity to create.</param>
    /// <returns>Created <see cref="User"/> with generated fields populated.</returns>
    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="user">User entity with updated values.</param>
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a user by id if it exists.
    /// </summary>
    /// <param name="userId">Identifier of the user to delete.</param>
    public async Task DeleteAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Checks whether an email is already registered.
    /// </summary>
    /// <param name="email">Email to check.</param>
    /// <returns>True if the email exists; otherwise false.</returns>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    /// <summary>
    /// Checks whether a Riot ID is already registered.
    /// </summary>
    /// <param name="riotId">Riot ID to check (GameName#TAG).</param>
    /// <returns>True if the Riot ID exists; otherwise false.</returns>
    public async Task<bool> RiotIdExistsAsync(string riotId)
    {
        return await _context.Users.AnyAsync(u => u.RiotId == riotId);
    }
}
