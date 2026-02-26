using DraftGapBackend.Application.Dtos.Admin;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DraftGapBackend.Tests.Services;

public class AdminServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;

    public AdminServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
    }

    [Fact]
    public async Task GetAllUsers_ReturnsUserList()
    {
        // Arrange
        var users = new List<User>
        {
            new()
            {
                UserId = Guid.NewGuid(),
                Email = "user1@example.com",
                PasswordHash = "hash1",
                RiotId = "User1#EUW",
                RiotPuuid = "puuid1",
                Region = "euw1",
                IsActive = true
            },
            new()
            {
                UserId = Guid.NewGuid(),
                Email = "user2@example.com",
                PasswordHash = "hash2",
                RiotId = "User2#NA",
                RiotPuuid = "puuid2",
                Region = "na1",
                IsActive = true
            }
        };

        _mockUserRepository
            .Setup(r => r.GetAllActiveUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _mockUserRepository.Object.GetAllActiveUsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Email.Should().Be("user1@example.com");
    }

    [Fact]
    public async Task GetUserById_ValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            RiotId = "TestUser#EUW",
            RiotPuuid = "test-puuid",
            IsActive = true
        };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _mockUserRepository.Object.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
    }
}
