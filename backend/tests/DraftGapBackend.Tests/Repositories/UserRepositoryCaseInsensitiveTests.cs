using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Data;
using DraftGapBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DraftGapBackend.Tests.Repositories;

/// <summary>
/// Tests de integración para verificar búsqueda case-insensitive REAL en UserRepository.
/// Usa InMemory database para simular BD real sin mocks permisivos.
/// </summary>
public class UserRepositoryCaseInsensitiveTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryCaseInsensitiveTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Theory]
    [InlineData("TestUser#EUW", "TestUser#EUW")] // Exact match
    [InlineData("TestUser#EUW", "testuser#euw")] // Lowercase
    [InlineData("TestUser#EUW", "TESTUSER#EUW")] // Uppercase
    [InlineData("TestUser#EUW", "TeStUsEr#EuW")] // Mixed case
    [InlineData("Faker#KR1", "faker#kr1")]       // Otro ejemplo
    [InlineData("Faker#KR1", "FAKER#KR1")]       // Otro ejemplo
    public async Task GetByRiotIdAsync_CaseInsensitiveSearch_FindsUser(string storedRiotId, string searchRiotId)
    {
        // Arrange: Insertar usuario en BD en memoria
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            RiotId = storedRiotId, // Almacenado con case específico
            Region = "euw1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act: Buscar con diferentes combinaciones de mayúsculas/minúsculas
        var result = await _repository.GetByRiotIdAsync(searchRiotId);

        // Assert: Debe encontrar el usuario independientemente del case
        Assert.NotNull(result);
        Assert.Equal(user.UserId, result.UserId);
        Assert.Equal(storedRiotId, result.RiotId); // Devuelve el RiotId original
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetByRiotIdAsync_UserNotFound_ReturnsNull()
    {
        // Arrange: Insertar un usuario
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            RiotId = "ExistingUser#EUW",
            Region = "euw1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act: Buscar usuario que no existe
        var result = await _repository.GetByRiotIdAsync("NonExistent#NA");

        // Assert: Debe retornar null
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByRiotIdAsync_MultipleUsersWithDifferentCase_FindsCorrectOne()
    {
        // Arrange: Insertar múltiples usuarios con RiotIds diferentes
        var user1 = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user1@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            RiotId = "UserOne#EUW",
            Region = "euw1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user2@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            RiotId = "UserTwo#EUW",
            Region = "euw1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act: Buscar user1 con lowercase
        var result = await _repository.GetByRiotIdAsync("userone#euw");

        // Assert: Debe encontrar user1, no user2
        Assert.NotNull(result);
        Assert.Equal(user1.UserId, result.UserId);
        Assert.Equal("UserOne#EUW", result.RiotId);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
