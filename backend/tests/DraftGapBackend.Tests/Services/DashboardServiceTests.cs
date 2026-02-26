using DraftGapBackend.Application.Dtos.Dashboard;
using DraftGapBackend.Application.Interfaces;
using DraftGapBackend.Domain.Abstractions;
using DraftGapBackend.Domain.Entities;
using DraftGapBackend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DraftGapBackend.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRankedRepository> _mockRankedRepository;
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<ILogger<DashboardService>> _mockLogger;
    private readonly IDashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRankedRepository = new Mock<IRankedRepository>();
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockLogger = new Mock<ILogger<DashboardService>>();

        _dashboardService = new DashboardService(
            _mockUserRepository.Object,
            _mockRankedRepository.Object,
            _mockMatchRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetDashboardSummary_ValidUser_ReturnsSummary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var puuid = "test-puuid-12345";

        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            RiotPuuid = puuid,
            RiotId = "TestUser#EUW"
        };

        var rankedStats = new List<PlayerRankedStat>
        {
            new()
            {
                Puuid = puuid,
                QueueType = "RANKED_SOLO_5x5",
                Tier = "GOLD",
                Rank = "II",
                LeaguePoints = 45,
                Wins = 50,
                Losses = 45
            }
        };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockRankedRepository
            .Setup(r => r.GetPlayerRankedStatsAsync(puuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rankedStats);

        _mockMatchRepository
            .Setup(r => r.GetUserMatchParticipantsAsync(puuid, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchParticipant>());

        _mockMatchRepository
            .Setup(r => r.GetUserMatchParticipantsAsync(puuid, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchParticipant>());

        _mockMatchRepository
            .Setup(r => r.GetUserMatchParticipantsAsync(puuid, 0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchParticipant>());

        // Act
        var result = await _dashboardService.GetDashboardSummaryAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.RankedOverview.Should().NotBeNull();
        result.RankedOverview!.SoloQueue.Should().NotBeNull();
        result.RankedOverview.SoloQueue!.Tier.Should().Be("GOLD");
        result.RankedOverview.SoloQueue.Rank.Should().Be("II");
    }

    [Fact]
    public async Task GetDashboardSummary_UserWithoutPuuid_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            RiotPuuid = null
        };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dashboardService.GetDashboardSummaryAsync(userId));
    }
}
