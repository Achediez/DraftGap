using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DraftGapBackend.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByRiotIdAsync(string riotId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RiotId != null && u.RiotId.ToLower() == riotId.ToLower());
    }

    public async Task<User?> GetByRiotPuuidAsync(string puuid)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RiotPuuid == puuid);
    }

    public async Task<IEnumerable<User>> GetAllActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersRequiringSyncAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive &&
                   (u.LastSync == null || u.LastSync < DateTime.UtcNow.AddHours(-24)))
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> RiotIdExistsAsync(string riotId)
    {
        return await _context.Users.AnyAsync(u => u.RiotId == riotId);
    }
}
