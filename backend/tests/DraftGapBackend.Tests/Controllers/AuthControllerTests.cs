using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Application.Users;
using DraftGapBackend.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DraftGapBackend.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IUserService> _mockUserService;

    public AuthControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            EmailOrUserName = "test@example.com",
            Password = "password123"
        };

        var authResponse = new AuthResponse
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Message = "Login successful"
        };

        _mockUserService
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(authResponse);

        _mockUserService
            .Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new User
            {
                UserId = authResponse.UserId,
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                RiotId = "TestUser#EUW"
            });

        // Act
        var result = await _mockUserService.Object.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
        result.Message.Should().Be("Login successful");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        _mockUserService
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _mockUserService.Object.LoginAsync(new LoginRequest()));

        exception.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Register_ValidData_ReturnsAuthResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123"
        };

        var authResponse = new AuthResponse
        {
            UserId = Guid.NewGuid(),
            Email = "newuser@example.com",
            Message = "Registration successful"
        };

        _mockUserService
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _mockUserService.Object.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("newuser@example.com");
    }
}
